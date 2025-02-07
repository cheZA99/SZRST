import { Component } from '@angular/core';
import {Router} from "@angular/router";
import {ToastrService} from "ngx-toastr";

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent {
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
