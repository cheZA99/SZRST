import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Employee {
  id: number;
  userName: string;
  email: string;
  firstName?: string;
  lastName?: string;
  jmbg?: string;
  active: boolean;
  isDeleted: boolean;
  tenantId?: number;
  tenantName: string;
  roles: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface EmployeeFilterParams {
  userName?: string;
  email?: string;
  firstName?: string;
  lastName?: string;
  tenantId?: number | null;
  isDeleted?: boolean;
  pageNumber?: number;
  pageSize?: number;
}

export interface CreateEmployeeDto {
  userName: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  jmbg?: string | null;
  password: string;
  confirmPassword: string;
  tenantId?: number;
}

export interface UpdateEmployeeDto {
  userName: string;
  email: string;
  firstName?: string | null;
  lastName?: string | null;
  jmbg?: string | null;
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

  getEmployees(
    filters: EmployeeFilterParams = {},
  ): Observable<PagedResult<Employee>> {
    let params = new HttpParams();

    if (filters.pageNumber)
      params = params.set('pageNumber', filters.pageNumber);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.userName?.trim())
      params = params.set('userName', filters.userName.trim());
    if (filters.email?.trim())
      params = params.set('email', filters.email.trim());
    if (filters.firstName?.trim())
      params = params.set('firstName', filters.firstName.trim());
    if (filters.lastName?.trim())
      params = params.set('lastName', filters.lastName.trim());
    if (filters.tenantId != null)
      params = params.set('tenantId', filters.tenantId);
    if (filters.isDeleted != null)
      params = params.set('isDeleted', filters.isDeleted);

    return this.http.get<PagedResult<Employee>>(`${this.apiUrl}/employees`, {
      params,
    });
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
