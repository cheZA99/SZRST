import { inject, Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { User } from '../types/user';
import { tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private baseUrl = 'https://localhost:5001/api/Auth/';
  currentUser = signal<User | null>(null);

  constructor(
    private http: HttpClient,
    private router: Router,
  ) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage(): void {
    const userStr = localStorage.getItem('user');
    if (userStr) {
      try {
        const user = JSON.parse(userStr);

        if (user.accessTokenExpires) {
          const expires = new Date(user.accessTokenExpires);
        }

        this.currentUser.set(user);
      } catch (e) {
        localStorage.removeItem('user');
      }
    }
  }

  login(loginObj: any) {
    return this.http.post<User>(`${this.baseUrl}login`, loginObj).pipe(
      tap((user) => {
        this.setCurrentUser(user);
      }),
    );
  }

  setCurrentUser(user: User) {
    const decoded = this.decodeToken(user.accessToken);

    if (!decoded) {
      console.error('Ne mogu dekodirati token');
      return;
    }

    const roles =
      decoded['role'] ||
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];

    let tenantId =
      decoded['tenantId'] ||
      decoded[
        'http://schemas.microsoft.com/ws/2008/06/identity/claims/tenantid'
      ];

    if (tenantId) {
      tenantId = +tenantId;
    }

    user.tenantId = tenantId || null;
    user.roles = Array.isArray(roles) ? roles : [roles];

    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
  }

  private decodeToken(token: string): any | null {
    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch (e) {
      console.error('Greška pri dekodiranju tokena:', e);
      return null;
    }
  }

  currentUserInfo(): any | null {
    const user = this.currentUser();
    if (!user) return null;

    const decoded = this.getDecodedToken();
    if (!decoded) return null;

    return {
      userName:
        decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] ||
        decoded['name'] ||
        user.userName ||
        '',
      userId: decoded.sub || decoded.userId || decoded.nameid,
      tenantId: user.tenantId,
      roles: user.roles || [],
    };
  }

  getAccessToken(): string | null {
    return this.currentUser()?.accessToken ?? null;
  }

  logout() {
    const user = this.currentUser();
    if (user?.refreshToken) {
      this.http
        .post(`${this.baseUrl}logout`, JSON.stringify(user.refreshToken), {
          headers: { 'Content-Type': 'application/json' },
        })
        .subscribe();
    }
    localStorage.removeItem('user');
    this.currentUser.set(null);
    this.router.navigate(['/login']);
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
        }),
      );
  }

  signUp(userObj: any) {
    return this.http.post<any>(`${this.baseUrl}register`, userObj).pipe(
      tap((user) => {
        if (user) this.setCurrentUser(user);
      }),
    );
  }

  getDecodedToken(): any | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = token.split('.')[1];
      return JSON.parse(atob(payload));
    } catch (e) {
      console.error('Greška pri dekodiranju tokena:', e);
      return null;
    }
  }

  getUserRoles(): string[] {
    const user = this.currentUser();
    return user?.roles || [];
  }

  hasRole(role: string): boolean {
    return this.getUserRoles().includes(role);
  }

  hasAnyRole(roles: string[]): boolean {
    return roles.some((r) => this.hasRole(r));
  }

  getTenantId(): number | null {
    const user = this.currentUser();
    return user?.tenantId || null;
  }
}
