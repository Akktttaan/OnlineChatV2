import {NgModule} from '@angular/core';
import {BrowserModule} from '@angular/platform-browser';
import {MatFormFieldModule} from "@angular/material/form-field";
import {HTTP_INTERCEPTORS, HttpClientModule} from "@angular/common/http";
import {MatInputModule} from "@angular/material/input";
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {RouterOutlet} from "@angular/router";

import {AppComponent} from './app.component';
import {AuthModule} from "./components/auth/auth.module";
import {API_BASE_URL, OnlineChatClient} from "../api/OnlineChatClient";
import {environment} from "../environments/environment";
import {ChatModule} from "./components/dashboard/chat/chat.module";
import {ChatRoutingModule} from "./components/dashboard/chat/chat-routing.module";
import {TokenInterceptor} from "./components/shared/classes/token.interceptor";
import {AppRoutingModule} from "./app-routing.module";
import {MatListModule} from "@angular/material/list";


@NgModule({
  declarations: [
    AppComponent,
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    AuthModule,
    ChatModule,
    ChatRoutingModule,
    MatFormFieldModule,
    AppRoutingModule,
    MatInputModule,
    MatListModule,
    HttpClientModule,
    RouterOutlet
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
