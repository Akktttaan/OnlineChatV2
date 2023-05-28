import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  // @ts-ignore
  private hubConnection: signalR.HubConnection;

  constructor() {
    this.initializeSignalR();
  }

  private initializeSignalR() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://your-signalr-api-url')
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR connection established.'))
      .catch(err => console.error('Error while establishing SignalR connection:', err));
  }

  public addChatMessageListener(callback: (message: string) => void) {
    this.hubConnection.on('ReceiveMessage', callback);
  }

  public sendMessage(message: string) {
    this.hubConnection.invoke('SendMessage', message)
      .catch(err => console.error('Error while sending message:', err));
  }
}
