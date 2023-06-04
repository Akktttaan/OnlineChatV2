export interface ChatModel{
  id: number;
  name: string;
  lastMessageFromSender: boolean;
  lastMessageSenderName: string;
  lastMessageText: string;
  lastMessageDate: Date | string;
}
