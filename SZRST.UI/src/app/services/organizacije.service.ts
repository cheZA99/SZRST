import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Organizacija {
  id: number;
  name: string;
  userCount: number;
}

export interface CreateOrganizacijaWithAdmin {
  tenantName: string;
  adminEmail: string;
  adminUsername: string;
  adminPassword: string;
  adminConfirmPassword: string;
}

export interface UpdateOrganizacija {
  name: string;
}

export interface TenantCreationResponse {
  isSuccess: boolean;
  message: string;
  tenant?: Organizacija;
  errors?: string[];
}

@Injectable({
  providedIn: 'root'
})
export class OrganizacijeService {
  private apiUrl = `${environment.apiUrl}/api/tenant`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Organizacija[]> {
    return this.http.get<Organizacija[]>(this.apiUrl);
  }

  getById(id: number): Observable<Organizacija> {
    return this.http.get<Organizacija>(`${this.apiUrl}/${id}`);
  }

  createWithAdmin(data: CreateOrganizacijaWithAdmin): Observable<TenantCreationResponse> {
    return this.http.post<TenantCreationResponse>(this.apiUrl, data);
  }

  update(id: number, organizacija: UpdateOrganizacija): Observable<Organizacija> {
    return this.http.put<Organizacija>(`${this.apiUrl}/${id}`, organizacija);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}