import { Component } from '@angular/core';
import {MatDialogRef} from "@angular/material/dialog";

@Component({
  selector: 'app-chat-profile-settings',
  templateUrl: './chat-profile-settings.component.html',
  styleUrls: ['./chat-profile-settings.component.sass']
})
export class ChatProfileSettingsComponent {

  constructor(private dialogRef: MatDialogRef<ChatProfileSettingsComponent>) {
  }

  close(){
    this.dialogRef.close()
  }
}
