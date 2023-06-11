import {Component, Inject} from '@angular/core';
import {OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";
import {MatCheckboxChange} from "@angular/material/checkbox";
import {SignalRService} from "../../../../shared/services/signalR.service";

@Component({
  selector: 'app-select-contact',
  templateUrl: './select-contact.component.html',
  styleUrls: ['./select-contact.component.sass']
})
export class SelectContactComponent {
  selectedContacts: number[] = [];
  chatOwnerId: number;

  constructor(@Inject(MAT_DIALOG_DATA) public data: any,
              private dialogRef: MatDialogRef<SelectContactComponent>,
              private api: OnlineChatClient,
              private signalR: SignalRService) {
    this.chatOwnerId = this.signalR.getChatOwnerId(this.data.chatId)
  }

  toggleContactSelection(event: MatCheckboxChange, contactId: number | undefined) {
    if (event.checked) {
      if (contactId != null) {
        this.selectedContacts.push(contactId);
      }
    } else {
      let index: number = 0;
      if (contactId != null) {
        index = this.selectedContacts.indexOf(contactId);
      }
      if (index !== -1) {
        this.selectedContacts.splice(index, 1);
      }
    }
  }

  close() {
    this.dialogRef.close(this.selectedContacts)
  }
}
