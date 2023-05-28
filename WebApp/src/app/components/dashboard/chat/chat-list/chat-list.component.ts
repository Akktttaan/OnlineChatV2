import {Component, ElementRef, ViewChild} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {SettingsComponent} from "../settings/settings.component";

@Component({
  selector: 'app-chat-list',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.sass']
})
export class ChatListComponent {
  chats = [
    { name: 'Chat 1', lastMessage: 'Last message 1', lastMessageTime: '15:22'},
    { name: 'Chat 2', lastMessage: 'Last message 2', lastMessageTime: '15:22' },
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
    { name: 'Chat 3', lastMessage: 'Last message 3', lastMessageTime: '15:22'},
  ];
  constructor(private dialog: MatDialog) {
  }

  openSettings() {
    const dialogRef = this.dialog.open(SettingsComponent, {
      autoFocus: false,
      width: '250px',
      position: {
        top: '60px',
        left: '10px'
      },
      backdropClass: 'dialog-backdrop',
      disableClose: false
    });

    dialogRef.afterClosed().subscribe(() => {
      // Действия после закрытия модального окна
    });
  }
}
