import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface City {
  id: number;
  name: string;
  country: {
    id: number;
    name: string;
    shortName: string;
  };
  isDeleted: boolean;
}

export interface CityCreateRequest {
  name: string;
  countryId: number;
  isDeleted: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class CityService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api`;

  getCities(): Observable<City[]> {
    return this.http.get<City[]>(`${this.baseUrl}/city`);
  }

  getCity(id: number): Observable<City> {
    return this.http.get<City>(`${this.baseUrl}/city/${id}`);
  }
    create(data: CityCreateRequest): Observable<City> {
      return this.http.post<City>(`${this.baseUrl}/city`, data);
    }
}