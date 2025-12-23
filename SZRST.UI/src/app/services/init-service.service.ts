import { inject, Injectable } from '@angular/core';
import { AuthService } from './auth.service';
import { of } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class InitServiceService {
  private authService = inject(AuthService);


  init() {
    const userString = localStorage.getItem('user');
    if (!userString) return of(null);
    const user = JSON.parse(userString);
    this.authService.currentUser.set(user);

    return of(null);
  }
}
