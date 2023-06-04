import {AfterViewInit, Component, EventEmitter, OnInit, Output} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {SettingsComponent} from "../dialogs/settings/settings.component";
import {ChatModel} from "./interfaces/chat-model";
import {SignalRService} from "../../../shared/services/signalR.service";
import {ActivatedRoute, Router} from "@angular/router";
import {ContactModel} from "../../../../../api/OnlineChatClient";

@Component({
  selector: 'app-chat-list',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.sass']
})
export class ChatListComponent implements OnInit, AfterViewInit {
  @Output() onClickChat = new EventEmitter<ChatModel>();
  chats: ChatModel[]
  clientId: string

  constructor(private dialog: MatDialog,
              private signalR: SignalRService,
              private router: Router,
              private route: ActivatedRoute) {
    this.clientId = this.route.snapshot.params['id']
  }

  ngOnInit(): void {

  }

  openSettings() {
    this.dialog.open(SettingsComponent, {
      autoFocus: false,
      width: '250px',
      position: {
        top: '60px',
        left: '10px'
      },
      backdropClass: 'dialog-backdrop',
      disableClose: false,
      data: {
        clientId: this.clientId
      }
    })
      .afterClosed()
      .subscribe(data => {
        if (data instanceof ContactModel) {
          this.onClickChat.emit({
            id: data.userId!,
            name: data.username!,
            lastMessageFromSender: false,
            lastMessageSenderName: '',
            lastMessageText: '',
            lastMessageDate: new Date()
          })
        }
      })

  }

  ngAfterViewInit(): void {
    this.signalR.connectionEstablished$
      .subscribe((state) => {
        if (state) {
          this.signalR.getUserChats(this.clientId)
        }
      })

    this.signalR.chats$
      .subscribe((res) => {
        this.chats = res
        console.log("чаты", res);
        // if (this.chats.length > 0) {
        //   this.openChat(this.chats[0].id)
        // }
      })

    this.signalR.pushNotifyMessages$
      .subscribe(res => {
        const chat = this.chats.find(x => x.id == res.chatId)
        if (chat) {
          const index = this.chats.indexOf(chat);
          this.chats.splice(index, 1)
          chat.lastMessageText = res.messageText;
          chat.lastMessageSenderName = res.sender.username;
          chat.lastMessageDate = res.messageDate;
          chat.lastMessageFromSender = parseInt(this.clientId) == res.sender.userId
          this.chats.unshift(chat)
        } else {
          this.chats.unshift({
            id: res.chatId,
            name: res.sender.userId != parseInt(this.clientId) ?
              res.chatId < 0 ?
                res.chatName : res.sender.username
              : res.chatName,
            lastMessageFromSender: false,
            lastMessageSenderName: res.sender.username,
            lastMessageText: res.messageText,
            lastMessageDate: res.messageDate
          })
        }
      })
  }

  openChat(id: number) {
    this.signalR.enterToChat(id)
    this.onClickChat.emit(this.chats.find(x => x.id == id))
  }
}
