import {Sender} from "./sender";

export interface Message {
  type: string;
  messageId: number;
  messageText: string;
  messageDate: Date | string;
  sender: Sender
}
