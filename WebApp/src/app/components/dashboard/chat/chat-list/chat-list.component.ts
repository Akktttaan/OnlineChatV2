import {AfterViewInit, ChangeDetectorRef, Component, EventEmitter, OnInit, Output} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {SettingsComponent} from "../dialogs/settings/settings.component";
import {ChatModel} from "./interfaces/chat-model";
import {SignalRService} from "../../../shared/services/signalR.service";
import {ActivatedRoute, Router} from "@angular/router";
import {ContactModel} from "../../../../../api/OnlineChatClient";
import {OnlineStatus} from "../../../shared/classes/online-status";
import {environment} from "../../../../../environments/environment";
import {FormBuilder} from "@angular/forms";

@Component({
  selector: 'app-chat-list',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.sass']
})
export class ChatListComponent implements OnInit, AfterViewInit {
  @Output() onClickChat = new EventEmitter<ChatModel>();
  chats: ChatModel[]
  clientId: string
  environment = environment;
  searchForm = this.builder.group({
    searchText: ['']
  });

  constructor(private dialog: MatDialog,
              private signalR: SignalRService,
              private router: Router,
              private route: ActivatedRoute,
              private builder: FormBuilder) {
    this.clientId = this.route.snapshot.params['id']
    this.search();
  }

  ngOnInit(): void {
    this.signalR.connectionEstablished$
      .subscribe((state) => {
        if (state) {
          this.signalR.getUserChats(this.clientId)
        }
      })

    this.signalR.chats$
      .subscribe((res) => {
        console.log("Чаты ", res)
        res.forEach(x => x.visible = true)
        this.chats = res
        this.signalR.chatsInChatList = this.chats
        this.getOnlineStatuses()
        // if (this.chats.length > 0) {
        //   this.openChat(this.chats[0].id)
        // }
      })

    this.signalR.leaveChatEventSbj
      .subscribe(x => {
        this.openChat(0, true);
      })

    this.signalR.setNewChatName$
      .subscribe(data => {
        const chat = this.chats.find(x => x.id == data.chatId)
        if(chat){
          chat.name = data.newName
        }
      })

    this.signalR.kickedFromChat$
      .subscribe(chatId => {
        const chat = this.chats.find(x => x.id == chatId)
        if(chat){
          this.chats.splice(this.chats.indexOf(chat), 1)
          this.openChat(0, true)
        }
      })
  }

  ngAfterViewInit(): void {
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
          chat.avatarUrl = res.avatarUrl
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
            lastMessageDate: res.messageDate,
            status: false,
            avatarUrl: res.avatarUrl,
            visible: true
          })
        }
      })
  }

  openSettings() {
    this.dialog.open(SettingsComponent, {
      autoFocus: false,
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
            name: data.userName!,
            lastMessageFromSender: false,
            lastMessageSenderName: '',
            lastMessageText: '',
            lastMessageDate: new Date(),
            status: false,
            avatarUrl: data.photoUrl!,
            visible: true
          })
        }
      })
  }

  openChat(id: number, isLeaveGroup: boolean) {
    if(!isLeaveGroup){
      this.signalR.enterToChat(id)
    }
    this.onClickChat.emit(this.chats.find(x => x.id == id))
  }

  getOnlineStatuses(){
    this.signalR.onlineStatus$
      .subscribe((onlineStatus: OnlineStatus) => {
        let contact = this.chats?.find(x => x.id == onlineStatus.userId)
        if(contact){
          contact.status = onlineStatus.status
        }
      })
  }

  search(){
    this.searchForm.controls.searchText.valueChanges
      .subscribe(searchText => {
        this.chats.forEach(x => x.visible = false)
        this.chats.forEach(x => {
          if(x.name.toLowerCase().includes(searchText?.toLowerCase()!)) x.visible = true;
        })
      })
  }

  clearSearchText() {
    this.searchForm.controls.searchText.setValue('')
  }
}
