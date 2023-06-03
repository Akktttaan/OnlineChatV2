import {AfterViewInit, Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";
import {FormArray, FormBuilder} from "@angular/forms";
import {SignalRService} from "../../../../shared/services/signalR.service";
import {ActivatedRoute} from "@angular/router";
import {ContactModel, OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {MatCheckboxChange} from "@angular/material/checkbox";
import {CreateChatModel} from "./interfaces/create-chat-model";

@Component({
  selector: 'app-group-create',
  templateUrl: './group-create.component.html',
  styleUrls: ['./group-create.component.sass']
})
export class GroupCreateComponent implements AfterViewInit{
  step: 'contactSelection' | 'groupSettings' = 'contactSelection';

  clientId: string;
  selectedContacts: number[] = [];

  contacts: Array<ContactModel>;
  selectedFile: File | null = null;
  dataForm = this.builder.group({
    chatName: [''],
    chatUserIds: [[]],
  })
  constructor(private dialogRef: MatDialogRef<GroupCreateComponent>,
              private builder: FormBuilder,
              private signalR: SignalRService,
              private route: ActivatedRoute,
              private api: OnlineChatClient,
              @Inject(MAT_DIALOG_DATA) public data: any) {
    this.clientId = data.clientId
    console.log(this.clientId)
  }
  changeStep(step: 'contactSelection' | 'groupSettings') {
    this.step = step;
  }

  onFileSelected(event: any): void {
    this.selectedFile = event.target.files[0];
  }

  ngAfterViewInit(): void {
    this.api.all2(parseInt(this.clientId))
      .subscribe(res => {
        this.contacts = res
      })
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

  onSubmit() {
    const model = new CreateChatModel()
    // @ts-ignore
    model.chatName = this.dataForm.value.chatName;
    model.createdById = this.clientId
    model.chatUserIds = this.selectedContacts
    model.chatUserIds.push(parseInt(this.clientId))
    console.log(model.chatUserIds)
    // @ts-ignore
    this.signalR.createChat(model)
    this.close()
  }

  close(){
    this.dialogRef.close()
  }
}
