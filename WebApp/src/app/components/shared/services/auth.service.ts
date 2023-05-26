import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private token: string | null = null;
  constructor() {

  }

  public login(){

  }

  public register(){

  }

  public setToken(token: string | null){
    this.token = token;
  }

  public getToken(){
    return this.token;
  }

  public isAuthenticated(): boolean{
    return !!this.token;
  }

  public logout(){
    this.setToken(null);
    localStorage.clear()
  }
}
