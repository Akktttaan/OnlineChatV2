import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {RouterModule, Routes} from "@angular/router";
import {ChatComponent} from "./chat/chat.component";
import {AuthGuard} from "../../shared/classes/auth-guard";

const routes: Routes = [
  {
    path: ':id/chat/:chatId',
    component: ChatComponent,
    canActivate: [AuthGuard]
  }
]

@NgModule({
  declarations: [],
  imports: [
    CommonModule,
    RouterModule.forRoot(routes)
  ]
})
export class ChatRoutingModule {}
