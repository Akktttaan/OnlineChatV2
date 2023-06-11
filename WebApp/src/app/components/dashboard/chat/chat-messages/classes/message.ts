import {Sender} from "./sender";
import {MessageType} from "./message-type";

export interface Message {
  type: string;
  messageId: number;
  messageText: string;
  messageDate: Date | string;
  sender: Sender
  messageType: MessageType
}
