import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface FacilityType {
  id: number;
  name: string;
  description: string;
  isDeleted: boolean;
}

export interface FacilityTypeCreateDto {
  name: string;
  description: string;
}

@Injectable({ providedIn: 'root' })
export class FacilityTypeService {
  private readonly apiUrl = '/api/facilitytype';

  constructor(private http: HttpClient) {}

  getAll(): Observable<FacilityType[]> {
    return this.http.get<FacilityType[]>(this.apiUrl);
  }

  getById(id: number): Observable<FacilityType> {
    return this.http.get<FacilityType>(`${this.apiUrl}/${id}`);
  }

  create(data: FacilityTypeCreateDto): Observable<FacilityType> {
    return this.http.post<FacilityType>(this.apiUrl, data);
  }

  update(id: number, data: FacilityTypeCreateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}