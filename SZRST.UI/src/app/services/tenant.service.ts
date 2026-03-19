import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface Tenant {
  id: number;
  name: string;
}

@Injectable({
  providedIn: 'root',
})
export class TenantService {
  private baseUrl = `${environment.apiUrl}/api/Tenant`;

  constructor(private http: HttpClient) {}

  getAllTenants(): Observable<Tenant[]> {
    return this.http.get<Tenant[]>(this.baseUrl);
  }

  getTenantById(id: number): Observable<Tenant> {
    return this.http.get<Tenant>(`${this.baseUrl}/${id}`);
  }
}
