import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../types/user';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl: string = 'https://localhost:5001/api/Auth/';
  currentUser = signal<User | null>(null);

  constructor(private http: HttpClient) { }

  signUp(userObj: any) {
    return this.http.post<any>(`${this.baseUrl}register`, userObj).pipe(
      tap(user => {
        if (user)
          this.setCurrentUser(user);
      })
    );
  }

  login(loginObj: any) {
    return this.http.post<any>(`${this.baseUrl}login`, loginObj).pipe(
      tap(user => {
        if (user)
          this.setCurrentUser(user);
      })
    );
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user))
    this.currentUser.set(user)
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }
}
