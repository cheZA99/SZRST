import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';

export interface FacilityResponse {
  id: number;
  name: string;
  facilityType: {
    id: number;
    name: string;
    description: string;
  };
  location: {
    id: number;
    address: string;
    addressNumber: string;
    city: {
      id: number;
      name: string;
      country: {
        id: number;
        name: string;
      };
    };
  };
  imageUrl: string;
  isDeleted: boolean;
  tenantId?: number;
}

export interface FacilityCreateDto {
  name: string;
  facilityTypeId: number;
  locationId: number;
  imageUrl: string;
}

export interface FacilityLocationCreateDto {
  name: string;
  facilityTypeId: number;
  address: string;
  addressNumber: string;
  countryId: number;
  cityId: number;
  imageUrl: string;
}

@Injectable({ providedIn: 'root' })
export class FacilityService {
  private readonly apiUrl = `${environment.apiUrl}/api/facility`;

  constructor(private http: HttpClient) {}

  getAll(filter?: string, value?: string): Observable<FacilityResponse[]> {
    let params = new HttpParams();

    if (filter && value) {
      params = params.set('filter', filter);
      params = params.set('value', value);
    }

    return this.http.get<FacilityResponse[]>(this.apiUrl, { params });
  }

  getById(id: number): Observable<FacilityResponse> {
    return this.http.get<FacilityResponse>(`${this.apiUrl}/${id}`);
  }

  create(data: FacilityCreateDto): Observable<FacilityResponse> {
    return this.http.post<FacilityResponse>(this.apiUrl, data);
  }

  createWithLocation(
    data: FacilityLocationCreateDto
  ): Observable<FacilityResponse> {
    return this.http.post<FacilityResponse>(`${this.apiUrl}/AddFacility`, data);
  }

  update(id: number, data: FacilityCreateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
