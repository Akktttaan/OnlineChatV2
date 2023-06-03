import {Component, Inject} from '@angular/core';
import {ContactOperationDto, OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {first} from "rxjs/operators";
import {MAT_DIALOG_DATA} from "@angular/material/dialog";
import {ChatModel} from "../../chat-list/interfaces/chat-model";

@Component({
  selector: 'app-chat-settings',
  templateUrl: './chat-settings.component.html',
  styleUrls: ['./chat-settings.component.sass']
})
export class ChatSettingsComponent {
  chat: ChatModel
  clientId: number
  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
              private api: OnlineChatClient) {
    this.chat = data.chat
    this.clientId = data.clientId
  }


  addContact() {
    const model = new ContactOperationDto()
    model.userId = this.clientId;
    model.contactId = this.chat.id;
    this.api.add(model)
      .pipe(first())
      .subscribe()
  }
}
