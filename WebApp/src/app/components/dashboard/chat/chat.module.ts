import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChatComponent } from './chat/chat.component';
import { ChatListComponent } from './chat-list/chat-list.component';
import { ChatMessagesComponent } from './chat-messages/chat-messages.component';
import { ChatProfileComponent } from './chat-profile/chat-profile.component';
import {MatGridListModule} from "@angular/material/grid-list";
import {MatButtonModule} from "@angular/material/button";
import {MatInputModule} from "@angular/material/input";
import {MatIconModule} from "@angular/material/icon";
import {MatSlideToggleModule} from "@angular/material/slide-toggle";
import {MatTabsModule} from "@angular/material/tabs";
import {MatSliderModule} from "@angular/material/slider";
import { SettingsComponent } from './settings/settings.component';
import {MatDialogModule} from "@angular/material/dialog";



@NgModule({
  declarations: [
    ChatComponent,
    ChatListComponent,
    ChatMessagesComponent,
    ChatProfileComponent,
    SettingsComponent
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
        MatDialogModule
    ]
})
export class ChatModule { }
