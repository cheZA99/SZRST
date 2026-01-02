import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from 'src/environments/environment';

export interface User {
  id: number;
  userName: string;
  email: string;
  active: boolean;
  isDeleted: boolean;
  tenantId?: number;
}

export interface UserCreateDto {
  userName: string;
  email: string;
  password: string;
  active: boolean;
  tenantId?: number;
}

export interface UserUpdateDto {
  userName: string;
  email: string;
  active: boolean;
  isDeleted: boolean;
  tenantId?: number;
}

export interface UserProfile {
  id: number;
  userName: string;
  email: string;
  imageUrl?: string;
  displayName?: string;
  dateOfBirth?: string;
  gender?: string;
  description?: string;
  cityId?: number;
  cityName?: string;
  countryId?: number;
  countryName?: string;
}

export interface UserProfileUpdate {
  userName: string;
  email: string;
  displayName?: string;
  dateOfBirth?: string;
  gender?: string;
  description?: string;
  imageUrl?: string;
  cityId?: number;
  countryId?: number;
  currentPassword?: string;
  newPassword?: string;
  confirmPassword?: string;
}

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/api/user`;

  private profileUpdated$ = new Subject<void>();

  constructor(private http: HttpClient) {}

  get profileUpdated() {
    return this.profileUpdated$.asObservable();
  }

  getAll(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  getById(id: number): Observable<User> {
    return this.http.get<User>(`${this.apiUrl}/${id}`);
  }

  getUsersForAppointments() {
    return this.http.get<User[]>(`${this.apiUrl}/for-appointments`);
  }

  create(data: UserCreateDto): Observable<any> {
    return this.http.post(this.apiUrl, data);
  }

  update(id: number, data: UserUpdateDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  getActiveUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  getCurrentUserProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/profile`);
  }

  updateCurrentUserProfile(profile: UserProfileUpdate): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/profile`, profile).pipe(
      tap(() => this.profileUpdated$.next())
    );
  }

  uploadProfileImage(base64Image: string): Observable<{ imageUrl: string }> {
    return this.http.post<{ imageUrl: string }>(
      `${this.apiUrl}/profile/upload-image`,
      { base64Image }
    ).pipe(
      tap(() => this.profileUpdated$.next())
    );
  }
}