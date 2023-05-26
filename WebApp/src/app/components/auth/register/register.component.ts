import {Component} from '@angular/core';
import {FormBuilder, Validators} from "@angular/forms";
import {AuthService} from "../../shared/services/auth.service";

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.sass']
})
export class RegisterComponent {
  dataForm = this.builder.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
    confirmedPassword: ['', [Validators.required]]
  })

  constructor(private builder: FormBuilder,
              private auth: AuthService) {

  }

  onSubmit() {
    this.auth.register()
  }
}
