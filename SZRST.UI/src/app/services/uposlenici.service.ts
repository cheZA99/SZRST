import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Employee {
  id: number;
  userName: string;
  email: string;
  active: boolean;
  isDeleted: boolean;
  tenantId?: number;
  tenantName: string;
  roles: string[];
}

export interface CreateEmployeeDto {
  userName: string;
  email: string;
  password: string;
  confirmPassword: string;
  tenantId?: number;
}

export interface UpdateEmployeeDto {
  userName: string;
  email: string;
  active: boolean;
  tenantId?: number;
  newPassword?: string;
  confirmPassword?: string;
}

export interface Tenant {
  id: number;
  name: string;
}

@Injectable({
  providedIn: 'root',
})
export class UposleniciService {
  private apiUrl = `${environment.apiUrl}/api/user`;

  constructor(private http: HttpClient) {}

  getEmployees(): Observable<Employee[]> {
    return this.http.get<Employee[]>(`${this.apiUrl}/employees`);
  }

  getEmployeeById(id: number): Observable<Employee> {
    return this.http.get<Employee>(`${this.apiUrl}/${id}`);
  }

  createEmployee(data: CreateEmployeeDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/create-employee`, data);
  }

  updateEmployee(id: number, data: UpdateEmployeeDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/update-employee/${id}`, data);
  }

  deleteEmployee(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
