import { Component } from '@angular/core';
import {ToastrService} from "ngx-toastr";
import {Router} from "@angular/router";

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent {
  activeSubMenu: string = ''; // To track the active submenu

  constructor(private router: Router, private toastr: ToastrService) {

  }

  toggleSubMenu(subMenu: string) {
    if (this.activeSubMenu === subMenu) {
      this.activeSubMenu = ''; // Collapse if already expanded
    } else {
      this.activeSubMenu = subMenu; // Expand if collapsed or different submenu clicked
    }
  }
}
