import { Injectable, EventEmitter } from '@angular/core';
import * as signalR from "@microsoft/signalr";
import { Message } from '../shared/message';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  statusChanged = new EventEmitter<Message>();  
  connectionEstablished = new EventEmitter<Boolean>();  
  
  private connectionIsEstablished = false;
  private _hubConnection: signalR.HubConnection

  constructor() { 
    this.createConnection();  
    this.registerOnServerEvents();  
    this.startConnection();  
  }

  private createConnection() {  
    this._hubConnection = new signalR.HubConnectionBuilder()  
      .withUrl('./messagehub')  
      .build();  
  }  
  
  private startConnection(): void {  
    this._hubConnection  
      .start()  
      .then(() => {  
        this.connectionIsEstablished = true;  
        console.log('Hub connection started');  
        this.connectionEstablished.emit(true);  
      })  
      .catch(err => {  
        console.log('Error while establishing connection, retrying...');  
        setTimeout(() => this.startConnection(), 5000);  
      });  
  }  
  
  private registerOnServerEvents(): void {  
    this._hubConnection.on('StatusChanged', (data: any) => {  
      console.log(data);
      this.statusChanged.emit(data);  
    });  
  }  
}
