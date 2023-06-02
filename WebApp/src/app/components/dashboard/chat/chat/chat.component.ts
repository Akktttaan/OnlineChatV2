import {Component} from '@angular/core';
import {MatDialog} from "@angular/material/dialog";
import {GroupCreateComponent} from "../dialogs/group-create/group-create.component";

@Component({
  selector: 'app-chat',
  templateUrl: './chat.component.html',
  styleUrls: ['./chat.component.sass']
})
export class ChatComponent {
  // @ts-ignore
  isDraggingOver: boolean;
  dragCounter: number = 0;

  showDropdown: boolean = false;
  isHovering: boolean = false;

  constructor(private dialog: MatDialog) {
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
    event.preventDefault();
    this.dragCounter++;
    this.isDraggingOver = true;
  }

  onFileDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
  }

  onFileDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragCounter--;
    if (this.dragCounter === 0) {
      this.isDraggingOver = false;
    }
  }

  onFileDrop(event: DragEvent): void {
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
      height: '600px'
    })
  }
}
