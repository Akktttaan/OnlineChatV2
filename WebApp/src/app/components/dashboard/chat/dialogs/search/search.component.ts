import {Component, Inject} from '@angular/core';
import {MAT_DIALOG_DATA, MatDialogRef} from "@angular/material/dialog";
import {ContactModel, ContactOperationDto, OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {Observable, ReplaySubject} from "rxjs";
import {ActivatedRoute, Router} from "@angular/router";
import {first} from "rxjs/operators";
import {FormBuilder, ɵElement, ɵValue} from "@angular/forms";
import {environment} from "../../../../../../environments/environment";

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.sass']
})
export class SearchComponent {
  contacts = new ReplaySubject<ContactModel[]>(1);
  clientId: number;
  dataForm = this.builder.group({
    searchText: ['']
  })

  environment = environment;
  constructor(private dialogRef: MatDialogRef<SearchComponent>,
              private api: OnlineChatClient,
              private route: ActivatedRoute,
              private router: Router,
              private builder: FormBuilder,
              @Inject(MAT_DIALOG_DATA) public data: any) {
    this.clientId = data.clientId
    this.api.all2(this.clientId)
      .pipe(first())
      .subscribe(res => {
        this.contacts.next(res);
      })
  }

  search(){
    // @ts-ignore
    this.api.search(this.dataForm.value.searchText)
      .pipe(first())
      .subscribe(res => {
        this.contacts.next(res)
      })
  }

  openChat(user: ContactModel){
    this.dialogRef.close(user)
  }
}
