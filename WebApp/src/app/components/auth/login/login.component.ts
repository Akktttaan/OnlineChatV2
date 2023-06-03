import {Component, OnInit} from '@angular/core';
import {FormBuilder, Validators} from "@angular/forms";
import {AuthService} from "../../shared/services/auth.service";
import {Router} from "@angular/router";
import {HttpClient, HttpErrorResponse} from "@angular/common/http";
import {BehaviorSubject, catchError, Subject, throwError} from "rxjs";
import {first} from "rxjs/operators";

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.sass']
})
export class LoginComponent implements OnInit {
  wrongLoginOrPassword = new BehaviorSubject<boolean>(false);

  dataForm = this.builder.group({
    nameOrEmail: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  constructor(private builder: FormBuilder,
              private auth: AuthService,
              private router: Router) {
  }

  ngOnInit(): void {

  }

  onSubmit() {
    this.dataForm.disable()
    // @ts-ignore
    this.auth.login(this.dataForm.getRawValue())
      .subscribe(
        (res) => this.router.navigate([res.id, 'chat', 'null']),
        error => {
          this.wrongLoginOrPassword.next(true)
          this.dataForm.enable();
        })
  }
}
