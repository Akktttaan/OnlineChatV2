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
        if(data instanceof ContactModel){
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
      })
  }

  openChat(id: number) {
    this.onClickChat.emit(this.chats.find(x => x.id == id))
  }
}
