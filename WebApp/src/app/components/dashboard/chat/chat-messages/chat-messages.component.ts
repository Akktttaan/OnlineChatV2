import {AfterViewInit, Component, ElementRef, Input, OnInit, Renderer2, ViewChild} from '@angular/core';
import {SignalRService} from "../../../shared/services/signalR.service";
import {Message} from "./classes/message";
import {ChatModel} from "../chat-list/interfaces/chat-model";
import {ChatSettingsComponent} from "../dialogs/chat-settings/chat-settings.component";
import {MatDialog} from "@angular/material/dialog";
import {ActivatedRoute} from "@angular/router";
import {filter, Subject} from "rxjs";
import {FormBuilder} from "@angular/forms";
import {NewMessage} from "./classes/new-message";
import {Sender} from "./classes/sender";

@Component({
  selector: 'app-chat-messages',
  templateUrl: './chat-messages.component.html',
  styleUrls: ['./chat-messages.component.sass']
})
export class ChatMessagesComponent implements AfterViewInit, OnInit {
  @Input() public chat: ChatModel
  @Input() public chatChanged: Subject<boolean>
  @ViewChild("chatList", {static: false}) chatListRef: ElementRef
  messages: Array<Message> = [];
  clientId: number

  dataForm = this.builder.group({
    messageText: ['']
  })
  showEmoji: any;

  constructor(private signalR: SignalRService,
              private dialog: MatDialog,
              private route: ActivatedRoute,
              private builder: FormBuilder,
              private renderer: Renderer2) {
    this.clientId = route.snapshot.params['id']

  }

  ngOnInit() {
    this.signalR.chatHistory$
      .pipe(
        filter(x => !!x)
      )
      .subscribe((data: Message[]) => {
        this.messages = []
        data.forEach(x => {
          if (x.sender.userId == this.clientId) {
            x.type = 'my'
            this.messages.push(x)
          } else {
            x.type = 'other'
            this.messages.push(x)
          }
        })
        this.scrollToBottom()
      })
    this.signalR.newMessages$
      .subscribe((data: NewMessage) => {
        if (data.chatId == this.chat.id) {
          data.message.type = 'other'
          this.messages.push(data.message)
          this.scrollToBottom('smooth')
        }
      })
  }

  ngAfterViewInit(): void {
  }

  scrollToBottom(behavior: ScrollBehavior = 'auto') {
    const nativeElement = this.chatListRef.nativeElement
    setTimeout(() => {
      nativeElement.scrollTo({
        top: nativeElement.scrollHeight,
        behavior: behavior
      });
    }, 1)
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

  send() {
    // @ts-ignore
    if (!(this.dataForm.value.messageText?.length > 0)) return
    const message = {
      messageText: this.dataForm.value.messageText!,
      messageId: 0,
      type: 'my',
      messageDate: new Date(),
      sender: new Sender(),
    }
    this.messages.push(message)
    this.signalR.send(this.dataForm.value.messageText!, this.chat.id, this.chat.name)
    this.dataForm.reset()
    this.scrollToBottom('smooth')
  }

  onEmojiSelectBtn($event: any) {
    if (this.dataForm.value.messageText) {
      this.dataForm.controls.messageText.setValue(this.dataForm.value.messageText + $event.emoji.native)
    } else {
      this.dataForm.controls.messageText.setValue($event.emoji.native)
    }
  }

  hideEmojiPicker() {
    setTimeout(() => {
      if (this.showEmoji) {
        this.showEmoji = false;
      }
    }, 200);
  }
}
