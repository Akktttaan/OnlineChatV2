import {Component, ElementRef, Input, ViewChild} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {SettingsComponent} from "../dialogs/settings/settings.component";
import {Observable} from "rxjs";
import {ChatModel} from "./interfaces/chat-model";

@Component({
  selector: 'app-chat-list',
  templateUrl: './chat-list.component.html',
  styleUrls: ['./chat-list.component.sass']
})
export class ChatListComponent {
  // @ts-ignore
  @Input() public chatList: Observable<ChatModel[]>
  // @ts-ignore
  chats: ChatModel[];
  constructor(private dialog: MatDialog) {
    // @ts-ignore
    this.chatList.subscribe(res => {
      this.chats = res
    })
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
