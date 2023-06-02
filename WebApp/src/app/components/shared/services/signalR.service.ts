import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import {AuthService} from "./auth.service";
import {HttpHeaders} from "@angular/common/http";

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  // @ts-ignore
  private hubConnection: signalR.HubConnection;
  private token = this.auth.getToken()

  constructor(private auth: AuthService) {

  }

  startConnection(): void {
    // @ts-ignore
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://26.47.201.227:8001/chat', { accessTokenFactory: () => this.token })
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connection started');
        // Дополнительные действия после успешного подключения
      })
      .catch(err => {
        console.error('Error while starting SignalR connection:', err);
      });
  }

  stopConnection(): void {
    this.hubConnection.stop();
  }

  addChatMessageListener(callback: (message: string) => void): void {
    this.hubConnection.on('ReceiveMessage', (message: string) => {
      callback(message);
    });
  }

  sendMessage(message: number): void {
    this.hubConnection.invoke('EnterToChat', message).then(r => console.log(r));
  }
}
