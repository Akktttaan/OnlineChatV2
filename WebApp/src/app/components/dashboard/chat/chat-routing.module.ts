import {NgModule} from "@angular/core";
import {CommonModule} from "@angular/common";

import {RouterModule, Routes} from "@angular/router";
import {ChatComponent} from "./chat/chat.component";

const routes: Routes = [
  {
    path: 'chat',
    component: ChatComponent
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
