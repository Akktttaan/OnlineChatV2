import {Injectable} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {HubConnectionBuilder, HubConnectionState, LogLevel} from '@microsoft/signalr';
import {AuthService} from "./auth.service";
import {ChatModel} from "../../dashboard/chat/chat-list/interfaces/chat-model";
import {BehaviorSubject, ReplaySubject, Subject} from "rxjs";
import {environment} from "../../../../environments/environment";
import {CreateChatModel} from "../../dashboard/chat/dialogs/group-create/interfaces/create-chat-model";
import {Message} from "../../dashboard/chat/chat-messages/classes/message";
import {ChatHistory} from "../../dashboard/chat/chat-messages/classes/chat-history";
import {Sender} from "../../dashboard/chat/chat-messages/classes/sender";
import {NewMessage} from "../../dashboard/chat/chat-messages/classes/new-message";

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;

  connectedChatHistories: Array<ChatHistory> = []
  chats$ = new Subject<ChatModel[]>();
  chatHistory$ = new ReplaySubject<Message[]>(1);
  connectionEstablished$ = new BehaviorSubject<boolean>(false);
  newMessages$ = new Subject<NewMessage>()

  clientId: number;
  constructor() {
    this.clientId = parseInt(localStorage.getItem('clientId')!)
  }

  start(token: string) {
    this.createConnection(token);
    this.registerOnServerEvents();
    this.startConnection();
  }

  private createConnection(token: string) {
    // @ts-ignore
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.apiUrl + '/chat', {accessTokenFactory: () => token})
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();
  }

  private startConnection() {
    if (this.hubConnection.state === HubConnectionState.Connected) {
      return;
    }

    this.hubConnection.start().then(
      () => {
        this.connectionEstablished$.next(true);
        console.log('Hub connection started!');
      },
      error => console.error(error)
    );

  }

  send(messageText: string, chatId: number){
    const message = {
      messageText: messageText,
      messageId: 0,
      type: 'my',
      messageDate: new Date(),
      sender: {
        username: '',
        userId: this.clientId,
        avatarUrl: ''
      },
    }
    this.connectedChatHistories.find(x => x.chatId == chatId)?.chatHistory.push(message)
    this.hubConnection.invoke('Send', {
      chatId: parseInt(chatId.toString()),
      messageText: messageText
    })
  }

  enterToChat(chatId: number): void {
    const chat = this.connectedChatHistories.find(x => x.chatId == chatId)
    if(chat) {
      this.chatHistory$.next(chat.chatHistory)
      return;
    }
    const newChat = new ChatHistory()
    newChat.chatId = chatId;
    this.connectedChatHistories.push(newChat)
    this.hubConnection.invoke('EnterToChat', parseInt(chatId.toString()))
  }

  getUserChats(userId: string): void {
    this.hubConnection.invoke('GetUserChats', parseInt(userId))
  }

  createChat(chatModel: CreateChatModel) {
    this.hubConnection
      .invoke('CreateChat', {
        chatName: chatModel.chatName,
        createdById: parseInt(chatModel.createdById),
        chatUserIds: chatModel.chatUserIds
      })
      .catch(err => console.error(err));
  }

  private registerOnServerEvents(): void {
    this.hubConnection.on('SetChats', (chats: ChatModel[]) => {
      this.chats$.next(chats)
    })

    this.hubConnection.on('SetChatHistory', (data: Message[]) => {
      const chat = this.connectedChatHistories[this.connectedChatHistories.length - 1]
      chat.chatHistory = data;
      this.chatHistory$.next(data)
    })

    this.hubConnection.on('ReceiveMessage', (chatId: number, message: Message) => {
      const newMessage = {
        chatId: chatId,
        message: message
      }
      this.connectedChatHistories.find(x => x.chatId == chatId)?.chatHistory.push(message)
      this.newMessages$.next(newMessage)
    });
  }

  deleteCashed(){
    this.connectedChatHistories = []
  }
}
