import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface Country {
  id: number;
  name: string;
  shortName: string;
  currencyId?: number;
  isDeleted: boolean;
}

export interface CountryCreateRequest {
  name: string;
  shortName: string;
  currencyId: number;
}

@Injectable({
  providedIn: 'root',
})
export class CountryService {
  private http = inject(HttpClient);
  private baseUrl = `${environment.apiUrl}/api`;

  getCountries(): Observable<Country[]> {
    return this.http.get<Country[]>(`${this.baseUrl}/country`);
  }

  getCountry(id: number): Observable<Country> {
    return this.http.get<Country>(`${this.baseUrl}/country/${id}`);
  }

  create(data: CountryCreateRequest): Observable<Country> {
    return this.http.post<Country>(`${this.baseUrl}/country`, data);
  }
}
