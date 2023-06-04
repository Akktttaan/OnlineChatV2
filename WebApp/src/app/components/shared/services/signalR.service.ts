import {Injectable} from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {HubConnectionBuilder, HubConnectionState, LogLevel} from '@microsoft/signalr';
import {ChatModel} from "../../dashboard/chat/chat-list/interfaces/chat-model";
import {BehaviorSubject, ReplaySubject, Subject, timer} from "rxjs";
import {environment} from "../../../../environments/environment";
import {CreateChatModel} from "../../dashboard/chat/dialogs/group-create/interfaces/create-chat-model";
import {Message} from "../../dashboard/chat/chat-messages/classes/message";
import {ChatHistory} from "../../dashboard/chat/chat-messages/classes/chat-history";
import {NewMessage} from "../../dashboard/chat/chat-messages/classes/new-message";
import {PushNotifyModel} from "../models/push-notify-model";
import {ChatInfo} from "../../dashboard/chat/chat/classes/chat-info";

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private timer: ReturnType<typeof setTimeout> | null = null;

  chatInfos: Array<ChatInfo> = []
  connectedChatHistories: Array<ChatHistory> = []
  chats$ = new Subject<ChatModel[]>();
  chatHistory$ = new ReplaySubject<Message[]>(1);
  chatInfo$ = new ReplaySubject<ChatInfo>(1)
  connectionEstablished$ = new BehaviorSubject<boolean>(false);
  newMessages$ = new Subject<NewMessage>()
  pushNotifyMessages$ = new Subject<PushNotifyModel>()

  clientId: number;

  constructor() {
    this.clientId = parseInt(localStorage.getItem('clientId')!)
  }

  start(token: string) {
    this.createConnection(token);
    this.registerOnServerEvents();
    this.startConnection();
    this.checkActive()
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
        this.hubConnection.invoke('Connect')
      },
      error => console.error(error)
    );
  }

  private stopConnection(){
    if(this.hubConnection){
      this.hubConnection.stop()
      console.log("Отключено от хаба");
      this.hubConnection.invoke('OnInactiveDisconnect')
    }
  }

  checkActive(){
    window.addEventListener("focus", this.focus.bind(this));
    window.addEventListener("blur", this.blur.bind(this));
  }

  focus(){
    if (!this.hubConnection) {
      this.startConnection();
    }
    this.resetTimer();
  }

  blur(){
    this.resetTimer();
  }

  resetTimer() {
    if(this.timer){
      clearTimeout(this.timer);
    }
    this.timer = setTimeout(this.stopConnection, 30000); // 30 секунд
  }

  send(messageText: string, chatId: number, chatName: string) {
    const message = {
      messageText: messageText,
      messageId: 0,
      type: 'my',
      messageDate: new Date(),
      sender: {
        username: '',
        userId: this.clientId,
        avatarUrl: '',
        nicknameColor: ''
      },
    }
    this.connectedChatHistories.find(x => x.chatId == chatId)?.chatHistory.push(message)
    this.pushNotifyMessages$.next({
      chatId: chatId,
      messageText: message.messageText,
      sender: message.sender,
      messageDate: message.messageDate,
      chatName: chatName
    })
    this.hubConnection.invoke('Send', {
      chatId: parseInt(chatId.toString()),
      messageText: messageText
    })
  }

  enterToChat(chatId: number): void {
    const chat = this.connectedChatHistories.find(x => x.chatId == chatId)
    const chatInfo = this.chatInfos.find(x => x.chatId == chatId)
    if (chat) {
      this.chatHistory$.next(chat.chatHistory)
      this.chatInfo$.next(chatInfo!)
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

    this.hubConnection.on('SetChatInfo', (data: ChatInfo) => {
      console.log("Информация о чатах ->", data)
      this.chatInfos.push(data)
      this.chatInfo$.next(data)
    })

    this.hubConnection.on('SetUserInfo', () => {

    })

    this.hubConnection.on('ReceiveMessage', (chatId: number, message: Message) => {
      const newMessage = {
        chatId: chatId,
        message: message
      }
      this.connectedChatHistories.find(x => x.chatId == chatId)?.chatHistory.push(message)
      this.newMessages$.next(newMessage)
    });

    this.hubConnection.on('PushNotify', (message: PushNotifyModel) => {
      console.log("Уведомление-> ", message)
      this.pushNotifyMessages$.next(message)
      this.sendNotification('Новое сообщение', {
        body: 'Вы получили новое сообщение',
        icon: 'path/to/icon.png'
      });
    })

    this.hubConnection.on('MessageDelivered', (data: any) => {
    })

    this.hubConnection.on('UserOnline', (id: number) => {
      console.log("Этот лох онлайн -> ", id)
    })

    this.hubConnection.on('UserOffline', (id: number, time: number) => {
      console.log("Этой хуй вышел из сети ", id, "в такое то время ", time)
    })
  }

  deleteCashed() {
    this.connectedChatHistories = []
  }

  sendNotification(title: string, options: NotificationOptions) {
    if (Notification.permission === 'granted') {
      new Notification(title, options);
    }
  }
}
