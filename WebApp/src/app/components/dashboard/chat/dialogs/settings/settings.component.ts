import {Component} from '@angular/core';
import {MatDialog, MatDialogRef} from "@angular/material/dialog";
import {AuthService} from "../../../../shared/services/auth.service";
import {Router} from "@angular/router";
import {SearchComponent} from "../search/search.component";

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.sass']
})
export class SettingsComponent {
  constructor(public dialogRef: MatDialogRef<SettingsComponent>,
              private auth: AuthService,
              private router: Router,
              private dialog: MatDialog) {
  }

  toggleDarkTheme() {
    const isDarkTheme = document.body.classList.contains('dark-mode');

    if (isDarkTheme) {
      document.body.classList.remove('dark-mode');
    } else {
      document.body.classList.add('dark-mode');
    }
  }

  async logout() {
    this.auth.logout()
    this.closeDialog()
    await this.router.navigate(['login'])
  }

  closeDialog() {
    this.dialogRef.close();
  }

  contacts() {
      this.dialog.open(SearchComponent, {
        width: '400px',
        height: '600px'
      })
      this.closeDialog()
  }
}
