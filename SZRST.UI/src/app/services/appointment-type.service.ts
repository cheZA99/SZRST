import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface AppointmentType {
  id: number;
  name: string;
  duration: number;
  price: number;
}

export interface AppointmentTypeCreateDto {
  name: string;
  duration: number;
  price: number;
}

@Injectable({ providedIn: 'root' })
export class AppointmentTypeService {
  private readonly apiUrl = `${environment.apiUrl}/api/appointmenttype`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AppointmentType[]> {
    return this.http.get<AppointmentType[]>(this.apiUrl);
  }

  getById(id: number): Observable<AppointmentType> {
    return this.http.get<AppointmentType>(`${this.apiUrl}/${id}`);
  }

  create(data: AppointmentTypeCreateDto): Observable<AppointmentType> {
    return this.http.post<AppointmentType>(this.apiUrl, data);
  }

  update(id: number, data: AppointmentTypeCreateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}