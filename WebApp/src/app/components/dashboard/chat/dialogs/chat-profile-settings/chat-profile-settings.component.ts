import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {FormBuilder} from "@angular/forms";
import {SelectContactComponent} from "../select-contact/select-contact.component";
import {SignalRService} from "../../../../shared/services/signalR.service";
import {OnlineChatClient, ChatInfo, ChatMember} from "../../../../../../api/OnlineChatClient";
import {environment} from "../../../../../../environments/environment";
import {FormFile} from "../group-create/interfaces/form-file";
import {fileToBase64} from "../../../../shared/functions/file-to-base64";
import {BehaviorSubject, ReplaySubject} from "rxjs";

@Component({
  selector: 'app-chat-profile-settings',
  templateUrl: './chat-profile-settings.component.html',
  styleUrls: ['./chat-profile-settings.component.sass']
})
export class ChatProfileSettingsComponent implements OnInit{
  initialChatInfo: ChatInfo;
  dataForm = this.builder.group({
    chatName: [''],
    chatDescription: ['']
  })
  imageSrc: any;
  private avatarFile: File;
  chatMembers = new ReplaySubject<ChatMember[]>(1);
  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
              private dialogRef: MatDialogRef<ChatProfileSettingsComponent>,
              private dialog: MatDialog,
              private builder: FormBuilder,
              private signalR: SignalRService,
              private api: OnlineChatClient) {
    this.imageSrc = environment.apiUrl + '/' + (data.avatarUrl ? data.avatarUrl : 'groupAvatar.png')
  }

  ngOnInit() {
    this.api.chatInfo(this.data.chatId)
      .subscribe(data => {
        console.log(data);
        this.dataForm.patchValue(data)
        this.initialChatInfo = data;
        this.imageSrc = environment.apiUrl + '/' + (data.avatarUrl ? data.avatarUrl : 'groupAvatar.png')
        this.chatMembers.next(data.members!)
      })
  }

  async onSubmit() {
    if(this.dataForm.value.chatName != this.initialChatInfo.chatName){
      this.signalR.updateChatName(this.dataForm.value.chatName!, this.data.chatId)
    }
    if(this.dataForm.value.chatDescription != this.initialChatInfo.chatDescription){
      this.signalR.updateAboutChat(this.dataForm.value.chatDescription!, this.data.chatId)
    }
    if(this.avatarFile){
      const model = new FormFile()
      model.name = this.avatarFile.name;
      model.data = await fileToBase64(this.avatarFile)
      this.signalR.updateChatAvatar(model, this.data.chatId)
    }
    this.data.dialogRef.close()
    this.dialogRef.close()
  }

  onFileSelected(event: any) {
    this.avatarFile = event.target.files[0];

    if (this.avatarFile) {
      const reader: FileReader = new FileReader();

      reader.onload = (e: any) => {
        this.imageSrc = e.target.result;
      };

      reader.readAsDataURL(this.avatarFile);
    }
  }

  deleteFromGroup() {
    this.dialog.open(SelectContactComponent, {
      width: '400px',
      height: '600px',
      data: {
        clientId: this.data.clientId,
        isRemoveUsers: true,
        chatId: this.data.chatId,
        contacts: this.chatMembers,
      }
    })
      .afterClosed()
      .subscribe((data: Array<number>) => {
        this.dialogRef.close()
        this.data.dialogRef.close()
        this.signalR.removeUsers(data, this.data.chatId)
      })
  }
}
