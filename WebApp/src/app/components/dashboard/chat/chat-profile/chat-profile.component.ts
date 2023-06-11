import {AfterViewInit, Component, Input, ViewChild} from '@angular/core';
import {ChatModel} from "../chat-list/interfaces/chat-model";
import {ChatInfo} from "../chat/classes/chat-info";
import {SignalRService} from "../../../shared/services/signalR.service";
import {OnlineChatClient, UserInfo} from "../../../../../api/OnlineChatClient";
import {environment} from "../../../../../environments/environment";
import {Subject} from "rxjs";
import {MatTabGroup} from "@angular/material/tabs";

@Component({
  selector: 'app-chat-profile',
  templateUrl: './chat-profile.component.html',
  styleUrls: ['./chat-profile.component.sass']
})
export class ChatProfileComponent implements AfterViewInit {
  @Input() public chat: ChatModel;
  @Input() public changeChatSbj: Subject<boolean>
  @ViewChild('tabGroup', {static: false}) tab: MatTabGroup;
  chatInfo: ChatInfo;
  contactInfo: UserInfo;
  environment = environment

  constructor(private signalR: SignalRService,
              private api: OnlineChatClient) {

  }

  ngAfterViewInit() {
    this.signalR.chatInfo$
      .subscribe((data: ChatInfo) => {
        this.chatInfo = data;
        this.tab.selectedIndex = 0;
      })
  }
}
