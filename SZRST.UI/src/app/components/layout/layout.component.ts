import { Component, inject, OnInit } from '@angular/core';
import { Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit {
  activeSubMenu: string = ''; // To track the active submenu
  protected authService = inject(AuthService);
  userName: string = 'userName';
  user: any = null;

  constructor(private router: Router, private toastr: ToastrService) {

  }
  ngOnInit() {
      this.setCurrentUser()
  }

  setCurrentUser() {
    const userString = localStorage.getItem('user');
    if (!userString) return;
     this.user = JSON.parse(userString);
  }

  toggleSubMenu(subMenu: string) {
    if (this.activeSubMenu === subMenu) {
      this.activeSubMenu = ''; // Collapse if already expanded
    } else {
      this.activeSubMenu = subMenu; // Expand if collapsed or different submenu clicked
    }
  }
}
