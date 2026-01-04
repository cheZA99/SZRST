import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface Currency {
  id: number;
  name: string;
  shortName: string;
}

@Injectable({ providedIn: 'root' })
export class CurrencyService {
  private readonly apiUrl = `${environment.apiUrl}/api/currency`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Currency[]> {
    return this.http.get<Currency[]>(this.apiUrl);
  }

  getById(id: number): Observable<Currency> {
    return this.http.get<Currency>(`${this.apiUrl}/${id}`);
  }

  create(data: { name: string; shortName: string }): Observable<Currency> {
    return this.http.post<Currency>(this.apiUrl, data);
  }

  update(id: number, data: Currency): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}