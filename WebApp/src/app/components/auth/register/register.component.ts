import {Component} from '@angular/core';
import {FormBuilder, Validators} from "@angular/forms";
import {AuthService} from "../../shared/services/auth.service";
import {Router} from "@angular/router";
import {RegisterDto} from "../../../../api/OnlineChatClient";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.sass']
})
export class RegisterComponent {
  dataForm = this.builder.group({
    username: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    confirmedPassword: ['', [Validators.required]]
  })

  constructor(private builder: FormBuilder,
              private auth: AuthService,
              private router: Router) {}

  async onSubmit() {
    this.dataForm.disable()
    // @ts-ignore
    this.auth.register(new RegisterDto(this.dataForm.getRawValue()))
      .subscribe(
        (res) => this.router.navigate([res.id, 'chat', 'null']),
        error => {
          console.error(error);
          this.dataForm.enable()
        }
      )
  }
}
