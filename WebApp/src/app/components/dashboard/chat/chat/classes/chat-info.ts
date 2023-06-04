import {ChatMember} from "./chat-member";

export class ChatInfo {
  chatId: number;
  chatName: string;
  chatDescription: string;
  ownerId: number;
  members: ChatMember[]
}

