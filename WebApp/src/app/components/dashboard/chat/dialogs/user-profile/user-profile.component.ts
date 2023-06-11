import {Component, Inject, OnInit} from '@angular/core';
import {FormBuilder} from "@angular/forms";
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";
import {FileModel, OnlineChatClient, UpdateAboutDto, UploadPhotoDto} from "../../../../../../api/OnlineChatClient";
import {environment} from "../../../../../../environments/environment";
import {fileToBase64} from "../../../../shared/functions/file-to-base64";

@Component({
  selector: 'app-user-profile',
  templateUrl: './user-profile.component.html',
  styleUrls: ['./user-profile.component.sass']
})
export class UserProfileComponent implements OnInit {
  clientId: number;
  avatarFile: File;
  dataForm = this.builder.group({
    username: [''],
    about: [''],
    avatarUrl: ['']
  })
  imageSrc: string | ArrayBuffer | null = null;

  environment = environment;

  constructor(private dialogRef: MatDialogRef<UserProfileComponent>,
              private builder: FormBuilder,
              @Inject(MAT_DIALOG_DATA) public data: any,
              private api: OnlineChatClient) {
    this.clientId = this.data.clientId
  }

  ngOnInit() {
    this.api.getUserInfo(this.clientId)
      .subscribe(data => {
        this.dataForm.patchValue(data)
        this.imageSrc = environment.apiUrl + '/' + (data.avatarUrl ? data.avatarUrl : 'userAvatar.png')
      })
  }

  async onSubmit() {
    if (this.dataForm.controls.about.touched) {
      const model = new UpdateAboutDto()
      model.userId = this.clientId;
      model.about = this.dataForm.value.about!
      this.api.updateAbout(model).subscribe()
    }
    if (this.avatarFile) {
      const model = new UploadPhotoDto()
      model.userId = this.clientId;
      model.photo = new FileModel()
      model.photo.name = this.avatarFile.name!;
      model.photo.data = await fileToBase64(this.avatarFile)
      this.api.uploadPhoto(model).subscribe()
    }

    this.data.dialogRef.close();
    this.dialogRef.close()
  }

  onFileSelected(event: any): void {
    this.avatarFile = event.target.files[0];

    if (this.avatarFile) {
      const reader: FileReader = new FileReader();

      reader.onload = (e: any) => {
        this.imageSrc = e.target.result;
      };

      reader.readAsDataURL(this.avatarFile);
    }
  }
}
