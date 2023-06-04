import {AfterViewInit, Component, EventEmitter, Signal} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {GroupCreateComponent} from "../dialogs/group-create/group-create.component";
import {ActivatedRoute} from "@angular/router";
import {ChatModel} from "../chat-list/interfaces/chat-model";
import {Subject} from "rxjs";
import {SignalRService} from "../../../shared/services/signalR.service";
import {AuthService} from "../../../shared/services/auth.service";

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.sass']
})
export class ChatComponent implements AfterViewInit{
  isDraggingOver: boolean;
  dragCounter: number = 0;

  showDropdown: boolean = false;
  isHovering: boolean = false;
  clientId: number

  currentChat: ChatModel
  chatChanged = new Subject<boolean>()

  constructor(private dialog: MatDialog,
              private route: ActivatedRoute,
              private signalR: SignalRService,
              private auth: AuthService) {
    this.clientId = this.route.snapshot.params['id']
  }

  ngAfterViewInit() {
    this.signalR.start(this.auth.getToken()!);
  }


  toggleDropdown() {
    this.showDropdown = !this.showDropdown;
  }

  changeStateNewChatButton(state: boolean) {
    if(!state){
      this.showDropdown = false;
    }
    this.isHovering = state;
  }

  onFileDragEnter(event: DragEvent): void {
    if(!this.currentChat) return
    event.preventDefault();
    this.dragCounter++;
    this.isDraggingOver = true;
  }

  onFileDragOver(event: DragEvent): void {
    if(!this.currentChat) return
    event.preventDefault();
    event.stopPropagation();
  }

  onFileDragLeave(event: DragEvent): void {
    if(!this.currentChat) return
    event.preventDefault();
    this.dragCounter--;
    if (this.dragCounter === 0) {
      this.isDraggingOver = false;
    }
  }

  onFileDrop(event: DragEvent): void {
    if(!this.currentChat) return
    event.preventDefault();
    this.dragCounter = 0;
    this.isDraggingOver = false;
    const files = event.dataTransfer?.files;
    if (files) {
      this.handleDroppedFiles(files);
    }
  }

  handleDroppedFiles(files: FileList): void {
    // Проход по списку файлов и выполнение необходимых операций
    for (let i = 0; i < files.length; i++) {
      const file = files[i];
      // Обработка файла
      console.log('Новый файл:', file);
    }
  }

  createGroup() {
    this.dialog.open(GroupCreateComponent, {
      width: '400px',
      height: '600px',
      data: {
        clientId: this.clientId
      }
    })
  }

  onClickChat($event: ChatModel) {
    this.currentChat = $event;
    this.signalR.enterToChat($event.id)
  }
}
