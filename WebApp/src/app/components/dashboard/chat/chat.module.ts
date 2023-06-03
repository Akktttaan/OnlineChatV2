import {NgModule} from '@angular/core';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {ChatComponent} from './chat/chat.component';
import {ChatListComponent} from './chat-list/chat-list.component';
import {ChatMessagesComponent} from './chat-messages/chat-messages.component';
import {ChatProfileComponent} from './chat-profile/chat-profile.component';
import {MatGridListModule} from "@angular/material/grid-list";
import {MatButtonModule} from "@angular/material/button";
import {MatInputModule} from "@angular/material/input";
import {MatIconModule} from "@angular/material/icon";
import {MatSlideToggleModule} from "@angular/material/slide-toggle";
import {MatTabsModule} from "@angular/material/tabs";
import {MatSliderModule} from "@angular/material/slider";
import {SettingsComponent} from './dialogs/settings/settings.component';
import {MatDialogModule} from "@angular/material/dialog";
import {GroupCreateComponent} from './dialogs/group-create/group-create.component';
import {MatCheckboxModule} from "@angular/material/checkbox";
import {MatListModule} from "@angular/material/list";
import {SearchComponent} from "./dialogs/search/search.component";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import { ChatProfileSettingsComponent } from './dialogs/chat-profile-settings/chat-profile-settings.component';
import {LastMessageTimePipe} from "../../shared/pipes/last-message-time.pipe";
import { ChatSettingsComponent } from './dialogs/chat-settings/chat-settings.component';


@NgModule({
  declarations: [
    ChatComponent,
    ChatListComponent,
    ChatMessagesComponent,
    ChatProfileComponent,
    SettingsComponent,
    GroupCreateComponent,
    SearchComponent,
    ChatProfileSettingsComponent,
    LastMessageTimePipe,
    ChatSettingsComponent
  ],
  imports: [
    CommonModule,
    MatGridListModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatSlideToggleModule,
    MatTabsModule,
    MatSliderModule,
    MatDialogModule,
    MatCheckboxModule,
    MatListModule,
    NgOptimizedImage,
    FormsModule,
    ReactiveFormsModule
  ]
})
export class ChatModule {
}
