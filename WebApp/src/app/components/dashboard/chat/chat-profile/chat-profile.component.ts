import {Component, Input} from '@angular/core';
import {ChatModel} from "../chat-list/interfaces/chat-model";

@Component({
  selector: 'app-chat-profile',
  templateUrl: './chat-profile.component.html',
  styleUrls: ['./chat-profile.component.sass']
})
export class ChatProfileComponent {
  @Input() public chat: ChatModel

  constructor() {

  }


}
