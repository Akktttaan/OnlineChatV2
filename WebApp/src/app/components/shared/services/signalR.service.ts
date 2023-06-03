import {Injectable} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {HubConnectionBuilder, HubConnectionState, LogLevel} from '@microsoft/signalr';
import {AuthService} from "./auth.service";
import {ChatModel} from "../../dashboard/chat/chat-list/interfaces/chat-model";
import {BehaviorSubject, Subject} from "rxjs";
import {environment} from "../../../../environments/environment";
import {CreateChatModel} from "../../dashboard/chat/dialogs/group-create/interfaces/create-chat-model";

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private connectionEstablishedPromise: Promise<void>;

  chats$ = new Subject<ChatModel[]>();
  connectionEstablished$ = new BehaviorSubject<boolean>(false);

  constructor(private auth: AuthService) {

  }

  start() {
    this.createConnection();
    this.registerOnServerEvents();
    this.startConnection();
  }

  private createConnection() {
    // @ts-ignore
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(environment.apiUrl + '/chat', {accessTokenFactory: () => this.auth.getToken()})
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
        console.log('Hub connection started!');
        this.connectionEstablished$.next(true);
      },
      error => console.error(error)
    );

  }

  addChatMessageListener(callback: (message: string) => void): void {
    this.hubConnection.on('ReceiveMessage', (message: string) => {
      callback(message);
    });
  }

  sendMessage(message: number): void {
    this.hubConnection.invoke('EnterToChat', message).then(r => console.log(r));
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
  }
}
