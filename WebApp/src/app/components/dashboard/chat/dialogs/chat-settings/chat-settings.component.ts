import {Component, Inject} from '@angular/core';
import {ContactOperationDto, OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {first} from "rxjs/operators";
import {MAT_DIALOG_DATA, MatDialog, MatDialogRef} from "@angular/material/dialog";
import {ChatModel} from "../../chat-list/interfaces/chat-model";
import {ChatProfileSettingsComponent} from "../chat-profile-settings/chat-profile-settings.component";
import {SignalRService} from "../../../../shared/services/signalR.service";
import {SelectContactComponent} from "../select-contact/select-contact.component";

@Component({
  selector: 'app-chat-settings',
  templateUrl: './chat-settings.component.html',
  styleUrls: ['./chat-settings.component.sass']
})
export class ChatSettingsComponent {
  chat: ChatModel
  clientId: number
  chatOwnerId: number | undefined;
  isMyFriend: boolean;

  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
              private dialogRef: MatDialogRef<ChatSettingsComponent>,
              private api: OnlineChatClient,
              private dialog: MatDialog,
              private signalR: SignalRService) {
    this.chat = data.chat
    this.clientId = data.clientId
    if (this.chat.id < 0) {
      this.chatOwnerId = this.signalR.getChatOwnerId(this.chat.id);
    } else {
      this.isMyFriend = !this.signalR.friendList.some(x => x.userId == this.chat.id)
    }
  }


  addContact() {
    const model = new ContactOperationDto()
    model.userId = this.clientId;
    model.contactId = this.chat.id;
    // @ts-ignore
    this.signalR.friendList.push({userId: model.contactId, userName: '', photoUrl: ''})
    this.api.add(model)
      .pipe(first())
      .subscribe(() => {
        this.dialogRef.close()
        this.isMyFriend = true;
      })
  }

  deleteContact() {
    const model = new ContactOperationDto()
    model.userId = this.clientId;
    model.contactId = this.chat.id;
    this.signalR.friendList.splice(
      this.signalR.friendList.indexOf(
        this.signalR.friendList.find(x => x.userId == model.contactId)!),
      1)
    this.api.delete(model)
      .pipe(first())
      .subscribe(() => {
        this.dialogRef.close()
        this.isMyFriend = false;
      })
  }

  openGroupSettings() {
    this.dialog.open(ChatProfileSettingsComponent, {
      data: {
        clientId: this.clientId,
        chatId: this.chat.id,
        dialogRef: this.dialogRef,
        avatarUrl: this.chat.avatarUrl
      }
    })
  }

  leaveGroup() {
    this.signalR.leaveChat(this.chat.id)
    this.dialogRef.close()
  }

  addToGroup() {
    this.dialog.open(SelectContactComponent, {
      width: '400px',
      height: '600px',
      data: {
        clientId: this.clientId,
        contacts: this.api.missingChatUsers(this.clientId, this.chat.id),
      }
    })
      .afterClosed()
      .subscribe((data: Array<number>) => {
        this.dialogRef.close()
        this.signalR.addPeopleToGroup(data, this.chat.id)
      })
  }
}
