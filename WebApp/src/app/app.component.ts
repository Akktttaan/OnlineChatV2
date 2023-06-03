import {Component, OnInit} from '@angular/core';
import {AuthService} from "./components/shared/services/auth.service";
import {Router} from "@angular/router";
import {SignalRService} from "./components/shared/services/signalR.service";

@Component({
  selector: 'app-root',
  template: `
    <router-outlet></router-outlet>`
})
export class AppComponent implements OnInit {
  constructor(private auth: AuthService,
              private router: Router,
              private signalR: SignalRService) {

  }

  async ngOnInit(): Promise<void> {
    const potentialToken = localStorage.getItem('auth-token')
    const tokenExpiredDate = localStorage.getItem('tokenExpiredDate')
    if (potentialToken !== null) {
      this.auth.setToken(potentialToken);
      // @ts-ignore
      this.auth.setExpiredDate(tokenExpiredDate)
    }
    const clientId = localStorage.getItem('clientId')
    if (this.auth.isAuthenticated()) {
      await this.router.navigate([clientId, 'chat', 'null'])
    } else {
      await this.router.navigate(['login']);
    }

    this.startSignalRService();
  }

  startSignalRService(){
  }
}
