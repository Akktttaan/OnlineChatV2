import {Component, Input, OnInit} from '@angular/core';
import {ChatModel} from "../chat-list/interfaces/chat-model";
import {ChatInfo} from "../chat/classes/chat-info";
import {SignalRService} from "../../../shared/services/signalR.service";

@Component({
  selector: 'app-chat-profile',
  templateUrl: './chat-profile.component.html',
  styleUrls: ['./chat-profile.component.sass']
})
export class ChatProfileComponent implements OnInit{
  @Input() public chat: ChatModel
  chatInfo: ChatInfo;
  constructor(private signalR: SignalRService) {

  }

  ngOnInit(): void {
    this.signalR.chatInfo$
      .subscribe((data: ChatInfo) => {
        console.log(data)
        this.chatInfo = data;
    })
  }


}
