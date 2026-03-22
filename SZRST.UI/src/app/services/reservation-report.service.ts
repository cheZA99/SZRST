import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { environment } from 'src/environments/environment';


export interface AppointmentReport {
    id: number;
    dateFrom: string;
    dateTo: string;
    createdAt: string;
    fileName: string;
    tenantId: number;
}

export interface AppointmentReportRequest {
    dateFrom: string;
    dateTo: string;
    tenantId: number;
}

@Injectable({ providedIn: 'root' })
export class AppointmentReportService {
    private readonly apiUrl = `${environment.apiUrl}/api/appointmentReport`;

    constructor(private http: HttpClient) { }

    getReports(): Observable<AppointmentReport[]> {
        return this.http.get<AppointmentReport[]>(`${this.apiUrl}`);
    }

    getReportsByTenantId(tenantId: number): Observable<AppointmentReport[]> {
        return this.http.get<AppointmentReport[]>(`${this.apiUrl}/${tenantId}`);
    }

    generateReport(data: AppointmentReportRequest): Observable<any> {
        return this.http.post(`${this.apiUrl}/generate`, data);
    }

    downloadReport(id: number) {
        return this.http.get(`${this.apiUrl}/download/${id}`, {
            responseType: 'blob'
        });
    }
}