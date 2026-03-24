import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from 'src/environments/environment';
import { logger } from 'src/app/utils/logger';

export interface Facility {
  id: number;
  name: string;
  facilityType: string;
  location: string;
}

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
  tenantName: string;
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
  locationId: number;
  tenantId: number;
  removeImage?: boolean;
}

export interface Tenant {
  id: number;
  name: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface FacilityFilterParams {
  name?: string;
  facilityTypeId?: string;
  address?: string;
  countryId?: number | null;
  cityId?: number | null;
  tenantId?: number | null;
  isDeleted?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortColumn?: string;
  sortDirection?: string;
}

@Injectable({ providedIn: 'root' })
export class FacilityService {
  private readonly apiUrl = `${environment.apiUrl}/api/facility`;

  constructor(private http: HttpClient) { }

  getAll(
    filters: FacilityFilterParams = {},
  ): Observable<PagedResult<FacilityResponse>> {
    let params = new HttpParams();

    if (filters.pageNumber)
      params = params.set('pageNumber', filters.pageNumber);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.name?.trim())
      params = params.set('name', filters.name.trim());
    if (filters.facilityTypeId != null)
      params = params.set('facilityTypeId', filters.facilityTypeId);
    if (filters.address?.trim())
      params = params.set('address', filters.address.trim());
    if (filters.countryId != null)
      params = params.set('countryId', filters.countryId);
    if (filters.cityId != null)
      params = params.set('cityId', filters.cityId);
    if (filters.tenantId != null)
      params = params.set('tenantId', filters.tenantId);
    if (filters.isDeleted != null)
      params = params.set('isDeleted', filters.isDeleted);
    if (filters.sortColumn?.trim())
      params = params.set('sortColumn', filters.sortColumn.trim());
    if (filters.sortDirection?.trim())
      params = params.set('sortDirection', filters.sortDirection.trim());

    logger.log("Params--->", params)

    return this.http.get<PagedResult<FacilityResponse>>(`${this.apiUrl}`, {
      params,
    });
  }

  getById(id: number): Observable<FacilityResponse> {
    return this.http.get<FacilityResponse>(`${this.apiUrl}/${id}`);
  }

  create(data: FacilityCreateDto): Observable<FacilityResponse> {
    return this.http.post<FacilityResponse>(this.apiUrl, data);
  }

  createWithLocation(
    data: FacilityLocationCreateDto,
    file: File | null
  ): Observable<FacilityResponse> {

    const formData = new FormData();

    formData.append("name", data.name);
    formData.append("facilityTypeId", data.facilityTypeId.toString());
    formData.append("address", data.address);
    formData.append("addressNumber", data.addressNumber);
    formData.append("countryId", data.countryId.toString());
    formData.append("cityId", data.cityId.toString());
    formData.append("tenantId", data.tenantId.toString());

    if (file) {
      formData.append("file", file);
    }

    logger.log("Data-->", data);
    logger.log("FormData-->", formData);

    return this.http.post<FacilityResponse>(`${this.apiUrl}/AddFacility`, formData);
  }

  updateWithLocation(
    id: number,
    data: FacilityLocationCreateDto,
    file: File | null,
    removeImage = false
  ): Observable<FacilityResponse> {

    const formData = new FormData();

    formData.append("name", data.name);
    formData.append("facilityTypeId", data.facilityTypeId.toString());
    formData.append("address", data.address);
    formData.append("addressNumber", data.addressNumber);
    formData.append("countryId", data.countryId.toString());
    formData.append("cityId", data.cityId.toString());

    if (data.tenantId) {
      formData.append("tenantId", data.tenantId.toString());
    }

    formData.append("locationId", data.locationId?.toString());
    formData.append("removeImage", removeImage.toString());

    if (file) {
      formData.append("file", file);
    }

    return this.http.put<FacilityResponse>(`${this.apiUrl}/${id}`, formData);
  }

  update(id: number, data: FacilityCreateDto): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}`, data);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
