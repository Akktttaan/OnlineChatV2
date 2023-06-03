import {AfterViewInit, Component, ElementRef, Input, ViewChild} from '@angular/core';
import {SignalRService} from "../../../shared/services/signalR.service";
import {Message} from "./classes/message";
import {ChatModel} from "../chat-list/interfaces/chat-model";
import {ChatSettingsComponent} from "../dialogs/chat-settings/chat-settings.component";
import {MatDialog} from "@angular/material/dialog";
import {ActivatedRoute} from "@angular/router";

@Component({
  selector: 'app-chat-messages',
  templateUrl: './chat-messages.component.html',
  styleUrls: ['./chat-messages.component.sass']
})
export class ChatMessagesComponent implements AfterViewInit {
  @Input() public chat: ChatModel
  @ViewChild('chatList') chatListRef: ElementRef;
  messages: Array<Message> = [
    {type: 'other', content: 'сам хуй'},
    {type: 'my', content: 'хуй'},

  ]
  clientId: number

  constructor(private signalR: SignalRService,
              private dialog: MatDialog,
              private route: ActivatedRoute) {
    this.clientId = route.snapshot.params['id']
  }

  send(number: number) {
    this.signalR.sendMessage(number)

  }

  ngAfterViewInit(): void {
    this.scrollToBottom();
  }

  scrollToBottom() {
    if (this.chatListRef && this.chatListRef.nativeElement) {
      const chatListElement = this.chatListRef.nativeElement;
      chatListElement.scrollTop = chatListElement.scrollHeight;
    }
  }

  openChatSettings() {
    this.dialog.open(ChatSettingsComponent, {
      autoFocus: false,
      width: '250px',
      position: {
        top: '60px',
        right: '25%'
      },
      backdropClass: 'dialog-backdrop',
      disableClose: false,
      data: {
        chat: this.chat,
        clientId: this.clientId
      }
    })
  }
}
