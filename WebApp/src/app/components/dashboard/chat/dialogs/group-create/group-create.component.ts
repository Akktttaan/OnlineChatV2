import { Component } from '@angular/core';
import {MatDialogRef} from "@angular/material/dialog";

@Component({
  selector: 'app-group-create',
  templateUrl: './group-create.component.html',
  styleUrls: ['./group-create.component.sass']
})
export class GroupCreateComponent {
  step: 'contactSelection' | 'groupSettings' = 'contactSelection';

  contacts = [
    { id: 1, name: 'Контакт 1', selected: false },
    { id: 2, name: 'Контакт 2', selected: false },
    { id: 3, name: 'Контакт 3', selected: false },
    // Добавьте другие контакты по мере необходимости
  ];

  groupName: string = '';
  selectedFile: File | null = null;

  constructor(private dialogRef: MatDialogRef<GroupCreateComponent>) {
  }
  changeStep(step: 'contactSelection' | 'groupSettings') {
    this.step = step;
  }

  createGroup(): void {
    // Проверка введенных данных и отправка события для создания группы
    if (this.groupName) {
      const groupData: object = {
        name: this.groupName,
        // Добавьте другие данные группы по мере необходимости
      };
    }
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  close(chatId: number){
    this.dialogRef.close(chatId)
  }
}
