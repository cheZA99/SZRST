import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

@Injectable({ providedIn: 'root' })
export class AppointmentService {
  private readonly apiUrl = `${environment.apiUrl}/api/appointment`;

  constructor(private http: HttpClient) {}

  getCalendar(
    from: string, 
    to: string, 
    facilityId?: number,
    tenantId?: number,
  ) {
    const params: any = {
      from,
      to
    };

    if (facilityId) {
      params.facilityId = facilityId;
    }

    if (tenantId) {
      params.tenantId = tenantId;
    }

    return this.http.get<any[]>(`${this.apiUrl}/calendar`, { params });
  }

  getById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  create(data: any) {
    return this.http.post(this.apiUrl, data);
  }

  update(id: number, data: any) {
    return this.http.put(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
getDashboardStats(): Observable<any> {
  return this.http.get<any>(`${this.apiUrl}/dashboard-stats`);
}
}

export interface AppointmentPayload {
  id?: number;
  appointmentDateTime: string;
  isFree: boolean;
  isClosed: boolean;
  facilityId: number;
  appointmentTypeId: number;
}