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
    MatFormFieldModule,
    MatInputModule,
  ],
  providers: [
    {provide: API_BASE_URL, useValue: environment.apiUrl},
    OnlineChatClient
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
}
