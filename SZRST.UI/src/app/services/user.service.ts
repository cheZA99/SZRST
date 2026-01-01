import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
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

@Injectable({
  providedIn: 'root',
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/api/user`;

  constructor(private http: HttpClient) {}

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
}