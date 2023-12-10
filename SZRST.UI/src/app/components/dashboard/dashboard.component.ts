import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent {
  activeSubMenu: string = ''; // To track the active submenu

  toggleSubMenu(subMenu: string) {
    if (this.activeSubMenu === subMenu) {
      this.activeSubMenu = ''; // Collapse if already expanded
    } else {
      this.activeSubMenu = subMenu; // Expand if collapsed or different submenu clicked
    }
  }
}
