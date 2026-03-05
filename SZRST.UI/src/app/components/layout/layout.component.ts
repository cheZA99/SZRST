import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { Router, RouterLinkActive } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { AuthService } from 'src/app/services/auth.service';
import { UserService } from 'src/app/services/user.service';
import { Subscription } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.css']
})
export class LayoutComponent implements OnInit, OnDestroy {

  activeSubMenu: string = ''; // To track the active submenu
  protected authService = inject(AuthService);
  private userService = inject(UserService);
  private router = inject(Router);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  userName: string = 'userName';
  user: any = null;
  userProfileImage: string | null = null;
  private profileSubscription?: Subscription;

  ngOnInit() {
    const savedLang = localStorage.getItem('lang') || 'bs';
    this.translate.use(savedLang);

    this.loadUserProfileImage();

    // Listen for profile updates
    this.profileSubscription = this.userService.profileUpdated.subscribe(() => {
      this.loadUserProfileImage();
    });
  }

  ngOnDestroy() {
    this.profileSubscription?.unsubscribe();
  }

  loadUserProfileImage() {
    if (this.authService.currentUser()) {
      this.userService.getCurrentUserProfile().subscribe({
        next: (profile) => {
          this.userProfileImage = profile.imageUrl || null;
        },
        error: (error) => {
          console.error('Error loading profile image:', error);
        }
      });
    }
  }

  editProfile() {
    this.router.navigate(['/uredi-profil']);
  }

  logout() {
    this.authService.logout();
    this.router.navigate(['login']);
  }

  toggleSubMenu(subMenu: string) {
    if (this.activeSubMenu === subMenu) {
      this.activeSubMenu = ''; // Collapse if already expanded
    } else {
      this.activeSubMenu = subMenu; // Expand if collapsed or different submenu clicked
    }
  }

  switchLanguage(lang: string) {
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
}