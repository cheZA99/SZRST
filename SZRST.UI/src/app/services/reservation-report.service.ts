import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from 'src/environments/environment';


export interface ReservationReport {
    id: number;
    dateFrom: string;
    dateTo: string;
    createdAt: string;
    fileName: string;
    tenantId: number;
}

export interface ReservationReportRequest {
    dateFrom: string;
    dateTo: string;
    tenantId: number;
}

@Injectable({ providedIn: 'root' })
export class ReservationReportService {
    private readonly apiUrl = `${environment.apiUrl}/api/reservationReport`;

    constructor(private http: HttpClient) { }

    getReports(): Observable<ReservationReport[]> {
        return this.http.get<ReservationReport[]>(`${this.apiUrl}`);
    }

    getReportsByTenantId(tenantId: number): Observable<ReservationReport[]> {
        return this.http.get<ReservationReport[]>(`${this.apiUrl}/${tenantId}`);
    }

    generateReport(data: ReservationReportRequest): Observable<any> {
        return this.http.post(`${this.apiUrl}/generate`, data);
    }

    downloadReport(id: number) {
        return this.http.get(`${this.apiUrl}/download/${id}`, {
            responseType: 'blob'
        });
    }
}