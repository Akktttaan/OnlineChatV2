import {Injectable} from '@angular/core';
import {AuthenticateResponse, LoginDto, OnlineChatClient, RegisterDto} from "../../../../api/OnlineChatClient";
import {first, tap} from "rxjs/operators";
import {SignalRService} from "./signalR.service";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private token: string | null | undefined = null;
  private tokenExpiredDate: Date;
  public userName: string;

  constructor(private api: OnlineChatClient,
              private signalR: SignalRService) {
  }

  public login(user: LoginDto) {
    return this.api.auth(user)
      .pipe(
        first(),
        tap((res: AuthenticateResponse) => {
          const expiredDate = Date.now() + (7 * 24 * 60 * 60 * 1000)
          // @ts-ignore
          localStorage.setItem('auth-token', res.token)
          // @ts-ignore
          localStorage.setItem('clientId', res.id)
          this.userName = res.username!
          // @ts-ignore
          localStorage.setItem('tokenExpiredDate', expiredDate)
          this.setToken(res.token)
          // @ts-ignore
          this.setExpiredDate(expiredDate);
        })
      )
  }

  public register(user: RegisterDto) {
    return this.api.register(user)
      .pipe(
        first(),
        tap(res => {
          const expiredDate = Date.now() + (7 * 24 * 60 * 60 * 1000)
          // @ts-ignore
          localStorage.setItem('auth-token', res.token)
          // @ts-ignore
          localStorage.setItem('clientId', res.id)
          this.userName = res.username!
          // @ts-ignore
          localStorage.setItem('tokenExpiredDate', expiredDate)
          this.setToken(res.token)
          // @ts-ignore
          this.setExpiredDate(expiredDate);
        })
      )
  }

  public verifyToken() {
    return this.api.verify()
  }

  public setToken(token: string | null | undefined) {
    this.token = token;
  }

  public getToken() {
    return this.token;
  }

  public setExpiredDate(date: Date) {
    this.tokenExpiredDate = date;
  }

  public isAuthenticated(): boolean {
    if (this.tokenExpiredDate < new Date()) {
      return false
    } else {
      return !!this.token;
    }
  }

  public logout() {
    this.setToken(null);
    localStorage.clear()
    this.signalR.deleteCashed()
    this.signalR.stopConnection()
  }
}
