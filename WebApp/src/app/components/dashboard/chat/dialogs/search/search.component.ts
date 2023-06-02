import { Component } from '@angular/core';
import {MatDialogRef} from "@angular/material/dialog";
import {ContactModel, OnlineChatClient} from "../../../../../../api/OnlineChatClient";
import {Observable, ReplaySubject} from "rxjs";
import {ActivatedRoute} from "@angular/router";
import {first} from "rxjs/operators";
import {FormBuilder, ɵElement, ɵValue} from "@angular/forms";

@Component({
  selector: 'app-search',
  templateUrl: './search.component.html',
  styleUrls: ['./search.component.sass']
})
export class SearchComponent {
  // @ts-ignore
  contacts = new ReplaySubject<ContactModel[]>(1);
  clientId: number;
  dataForm = this.builder.group({
    searchText: ['']
  })
  constructor(private dialogRef: MatDialogRef<SearchComponent>,
              private api: OnlineChatClient,
              private route: ActivatedRoute,
              private builder: FormBuilder) {
    this.clientId = this.route.snapshot.params['clientId'];
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
        console.log(res)
        this.contacts.next(res)
      })
  }

  close(){
    this.dialogRef.close()
  }
}
