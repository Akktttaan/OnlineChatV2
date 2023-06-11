import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {FormBuilder, Validators} from "@angular/forms";
import {SignalRService} from "../../../../shared/services/signalR.service";
import {ActivatedRoute} from "@angular/router";
import {OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {CreateChatModel} from "./interfaces/create-chat-model";
import {SelectContactComponent} from "../select-contact/select-contact.component";
import {FormFile} from "./interfaces/form-file";
import {fileToBase64} from "../../../../shared/functions/file-to-base64";

@Component({
  selector: 'app-group-create',
  templateUrl: './group-create.component.html',
  styleUrls: ['./group-create.component.sass']
})
export class GroupCreateComponent implements OnInit {
  clientId: string;

  avatarFile: File | null = null;
  dataForm = this.builder.group({
    chatName: ['', [Validators.required]],
    description: [''],
  })
  private selectedContacts: Array<number>;
  contactSelected: boolean = false;

  constructor(private dialogRef: MatDialogRef<GroupCreateComponent>,
              private dialog: MatDialog,
              private builder: FormBuilder,
              private signalR: SignalRService,
              private route: ActivatedRoute,
              private api: OnlineChatClient,
              @Inject(MAT_DIALOG_DATA) public data: any) {
    this.clientId = data.clientId
  }

  onFileSelected(event: any): void {
    this.avatarFile = event.target.files[0];
  }

  async onSubmit() {
    if (this.dataForm.invalid) return
    const model = new CreateChatModel()
    // @ts-ignore
    model.chatName = this.dataForm.value.chatName;
    model.createdById = this.clientId
    model.chatUserIds = this.selectedContacts
    model.description = this.dataForm.value.description!;
    if (this.avatarFile) {
      model.avatar = new FormFile()
      model.avatar.data = await fileToBase64(this.avatarFile!);
      model.avatar.name = this.avatarFile?.name!
    }
    model.chatUserIds.push(parseInt(this.clientId))
    this.signalR.createChat(model)
    this.close()
  }

  close() {
    this.dialogRef.close()
  }

  ngOnInit(): void {
    this.dialog.open(SelectContactComponent, {
      width: '400px',
      height: '600px',
      disableClose: true,
      data: {
        clientId: this.clientId,
        contacts: this.api.all2(parseInt(this.data.clientId))
      }
    })
      .afterClosed()
      .subscribe((data: Array<number>) => {
        this.selectedContacts = data
        this.contactSelected = true;
      })
  }
}
