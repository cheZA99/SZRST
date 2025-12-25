import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../types/user';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = 'https://localhost:5001/api/Auth/';
  currentUser = signal<User | null>(null);

  constructor(private http: HttpClient) {}

  login(loginObj: any) {
    return this.http.post<User>(`${this.baseUrl}login`, loginObj).pipe(
      tap((user) => {
        this.setCurrentUser(user);
      })
    );
  }

  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  getAccessToken(): string | null {
    return this.currentUser()?.accessToken ?? null;
  }

  logout() {
    localStorage.removeItem('user');
    this.currentUser.set(null);
  }

  refreshToken() {
    const user = this.currentUser();
    if (!user) return null;

    return this.http
      .post<User>(`${this.baseUrl}refresh-token`, {
        accessToken: user.accessToken,
        refreshToken: user.refreshToken,
      })
      .pipe(
        tap((newUser) => {
          this.setCurrentUser(newUser);
        })
      );
  }

  signUp(userObj: any) {
    return this.http.post<any>(`${this.baseUrl}register`, userObj).pipe(
      tap((user) => {
        if (user) this.setCurrentUser(user);
      })
    );
  }
}
