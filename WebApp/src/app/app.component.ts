import {Component, OnInit} from '@angular/core';
import {AuthService} from "./components/shared/services/auth.service";
import {Router} from "@angular/router";

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>`
})
export class AppComponent implements OnInit {
  constructor(private auth: AuthService,
              private router: Router) {

  }

  ngOnInit(): void {
    const potentialToken = localStorage.getItem('auth-token')
    if (potentialToken !== null) {
      this.auth.setToken(potentialToken);
    }
  }
}
