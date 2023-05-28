import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';

import {AppComponent} from './app.component';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {RouterModule, Routes} from "@angular/router";
import {AuthModule} from "./components/auth/auth.module";
import {AuthRoutingModule} from "./components/auth/auth-routing.module";
import {MatFormFieldModule} from "@angular/material/form-field";
import {MatInputModule} from "@angular/material/input";
import {API_BASE_URL, OnlineChatClient} from "../api/OnlineChatClient";
import {environment} from "../environments/environment";
import {ChatModule} from "./components/dashboard/chat/chat.module";
import {ChatRoutingModule} from "./components/dashboard/chat/chat-routing.module";
import {HTTP_INTERCEPTORS, HttpClientModule} from "@angular/common/http";
import {TokenInterceptor} from "./components/shared/classes/token.interceptor";

const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  }
]

@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    RouterModule.forRoot(routes),
    AuthModule,
    AuthRoutingModule,
    ChatModule,
    ChatRoutingModule,
    MatFormFieldModule,
    MatInputModule,
    HttpClientModule
  ],
  providers: [
    {provide: API_BASE_URL, useValue: environment.apiUrl},
    {provide: HTTP_INTERCEPTORS, multi: true, useClass: TokenInterceptor},
    OnlineChatClient
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
