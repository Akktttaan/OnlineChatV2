import {Injectable} from '@angular/core';
import {LoginDto, OnlineChatClient, RegisterDto} from "../../../../api/OnlineChatClient";
import {first, tap} from "rxjs/operators";

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private token: string | null | undefined = null;

  constructor(private api: OnlineChatClient) {
  }

  public login(user: LoginDto) {
    return this.api.auth(user)
      .pipe(
        first(),
        tap(res => {
          // @ts-ignore
          localStorage.setItem('auth-token', res.token)
          this.setToken(res.token)
        })
      )
  }

  public register(user: RegisterDto) {
    return this.api.register(user)
      .pipe(
        first(),
        tap(res => {
          // @ts-ignore
          localStorage.setItem('auth-token', res.token)
          this.setToken(res.token)
        })
      )
  }

  public verifyToken(){
    return this.api.verify()
  }

  public setToken(token: string | null | undefined) {
    this.token = token;
  }

  public getToken() {
    return this.token;
  }

  public isAuthenticated(): boolean {
    return !!this.token;
  }

  public logout() {
    this.setToken(null);
    localStorage.clear()
  }
}
