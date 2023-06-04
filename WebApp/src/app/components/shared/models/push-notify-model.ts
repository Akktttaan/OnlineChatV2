import {Sender} from "../../dashboard/chat/chat-messages/classes/sender";

export class PushNotifyModel {
  chatId: number;
  messageText: string;
  sender: Sender;
  messageDate: Date | string;
  chatName: string
}
