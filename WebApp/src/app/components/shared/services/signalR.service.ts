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
import {OnlineStatus} from "../classes/online-status";
import {MessageType} from "../../dashboard/chat/chat-messages/classes/message-type";
import {FormFile} from "../../dashboard/chat/dialogs/group-create/interfaces/form-file";
import {ContactModel} from "../../../../api/OnlineChatClient";

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: signalR.HubConnection;
  private timer: ReturnType<typeof setTimeout> | null = null;

  public currentChat: ChatModel;
  public chatsInChatList: ChatModel[];
  public friendList: ContactModel[];

  leaveChatEventSbj = new Subject<boolean>()
  chatInfos: Array<ChatInfo> = []
  connectedChatHistories: Array<ChatHistory> = []

  chats$ = new Subject<ChatModel[]>();
  chatHistory$ = new ReplaySubject<Message[]>(1);
  chatInfo$ = new ReplaySubject<ChatInfo>(1)
  connectionEstablished$ = new BehaviorSubject<boolean>(false);
  newMessages$ = new Subject<NewMessage>()
  pushNotifyMessages$ = new Subject<PushNotifyModel>()
  onlineStatus$ = new ReplaySubject<OnlineStatus>()
  setNewChatName$ = new Subject<{chatId: number, newName: string}>()
  kickedFromChat$ = new Subject<number>()

  clientId: number;

  constructor() {
    this.clientId = parseInt(localStorage.getItem('clientId')!)
  }

  async start(token: string) {
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

  public stopConnection(){
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
      messageType: MessageType.Common
    }
    this.connectedChatHistories.find(x => x.chatId == chatId)?.chatHistory.push(message)
    this.pushNotifyMessages$.next({
      chatId: chatId,
      messageText: message.messageText,
      sender: message.sender,
      messageDate: message.messageDate,
      chatName: chatName,
      avatarUrl: this.currentChat.avatarUrl
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
        chatUserIds: chatModel.chatUserIds,
        avatar: chatModel.avatar,
        description: chatModel.description
      })
      .catch(err => console.error(err));
  }

  addPeopleToGroup(contactIds: Array<number>, chatId: number){
    if(!contactIds) return
    this.hubConnection.invoke('AddUser', chatId, contactIds)
  }

  leaveChat(chatId: number){
    this.chatsInChatList.splice(
      this.chatsInChatList.indexOf(this.chatsInChatList.find(x => x.id == chatId)!), 1
    )
    this.leaveChatEventSbj.next(true)
    this.hubConnection.invoke("LeaveChat", chatId)
  }

  removeUsers(userIds: Array<number>, chatId: number){
    if(!userIds) return
    this.hubConnection.invoke("RemoveUser", chatId, userIds)
  }

  updateChatAvatar(avatar: FormFile, chatId: number){
    this.hubConnection.invoke("ChangeChatAvatar", chatId, avatar)
  }

  updateAboutChat(aboutText: string, chatId: number){
    this.hubConnection.invoke("UpdateAbout", chatId, aboutText)
  }

  updateChatName(chatName: string, chatId: number){
    this.hubConnection.invoke("ChangeChatName", chatId, chatName)
  }

  private registerOnServerEvents(): void {
    this.hubConnection.on('SetChats', (chats: ChatModel[]) => {
      this.chats$.next(chats)
    })

    this.hubConnection.on('SetChatHistory', (data: Message[]) => {
      const chat = this.connectedChatHistories[this.connectedChatHistories.length - 1]
      chat.chatHistory = data;
      this.chatHistory$.next(data)
      console.log("История чатов", chat.chatHistory)
    })

    this.hubConnection.on('SetChatInfo', (data: ChatInfo) => {
      this.chatInfos.push(data)
      this.chatInfo$.next(data)
    })

    this.hubConnection.on('SetUserInfo', () => {

    })

    this.hubConnection.on('ReceiveMessage', (chatId: number, message: Message) => {
      console.log("ReceiveMessage в чате -", chatId, message)
      const newMessage = {
        chatId: chatId,
        message: message
      }
      this.connectedChatHistories.find(x => x.chatId == chatId)?.chatHistory.push(message)
      this.newMessages$.next(newMessage)
    });

    this.hubConnection.on('PushNotify', (message: PushNotifyModel) => {
      console.log("Уведомление", message)
      this.pushNotifyMessages$.next(message)
      this.sendNotification('Новое сообщение', {
        body: 'Вы получили новое сообщение',
        icon: 'path/to/icon.png'
      });
    })

    this.hubConnection.on('MessageDelivered', (data: any) => {
    })

    this.hubConnection.on('UserOnline', (id: number) => {
      this.onlineStatus$.next({userId: id, status: true})
    })

    this.hubConnection.on('UserOffline', (id: number, time: number) => {
      this.onlineStatus$.next({userId: id, status: false})
    })

    this.hubConnection.on('SetUsersOnline', (ids: Array<number>) => {
      ids.forEach(x => {
        this.onlineStatus$.next({userId: x, status: true});
      })
    })

    this.hubConnection.on("KickedFromChat", (chatId: number) => {
      this.kickedFromChat$.next(chatId)
    })

    this.hubConnection.on('SetChatName', (chatId: number, newName: string) => {
      console.log("Изменение название чата в чате ", chatId, "на ", newName)
      this.setNewChatName$.next({chatId: chatId, newName: newName})
    })

    this.hubConnection.on('AboutChanged', (chatId:number, about: string) => {
      let oldChatInfo = this.chatInfos.find(x => x.chatId == chatId)
      const model = new ChatInfo()
      model.chatId = chatId;
      model.ownerId = oldChatInfo?.ownerId!;
      model.chatName = oldChatInfo?.chatName!;
      model.members = oldChatInfo?.members!;
      model.chatDescription = about;
      this.chatInfos.splice(this.chatInfos.indexOf(oldChatInfo!),1)
      this.chatInfos.push(model);
      this.chatInfo$.next(model);
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

  getChatOwnerId(chatId: number): number{
    return this.chatInfos.find(x => x.chatId == chatId)?.ownerId!;
  }
}
