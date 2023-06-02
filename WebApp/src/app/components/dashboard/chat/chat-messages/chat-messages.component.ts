import {AfterViewInit, Component, ElementRef, ViewChild} from '@angular/core';
import {SignalRService} from "../../../shared/services/signalR.service";
import {ReplaySubject} from "rxjs";
import {Message} from "./classes/message";

@Component({
  selector: 'app-chat-messages',
  templateUrl: './chat-messages.component.html',
  styleUrls: ['./chat-messages.component.sass']
})
export class ChatMessagesComponent implements AfterViewInit {
  // @ts-ignore
  @ViewChild('chatList') chatListRef: ElementRef;
  messages: Array<Message> = [
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
    { type: 'other', content: 'сам хуй'},
    { type: 'my', content: 'хуй'},
  ]

  constructor(private signalR: SignalRService) {
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
}
