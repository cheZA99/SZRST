import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from 'src/app/services/auth.service';
import { ReservationReport, ReservationReportService } from 'src/app/services/reservation-report.service';
import { Tenant, TenantService } from 'src/app/services/tenant.service';

@Component({
  selector: 'app-izvjestaji',
  templateUrl: './izvjestaji.component.html',
  styleUrls: ['./izvjestaji.component.css']
})
export class IzvjestajiComponent implements OnInit {

  tenants: Tenant[] = [];
  reports: ReservationReport[] = [];
  filteredReports: ReservationReport[] = [];

  loading = false;
  loadingTenants = false;
  showModal = false;

  isSuperAdmin = false;
  isAdmin = false;

  currentUserTenantId: number | null = null;

  reportForm: FormGroup;

  // Filter varijable
  selectedTenantFilter: number | null = null;

  ngOnInit(): void {
    this.checkPermissions();
    this.loadReports();

    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;

    if (this.isSuperAdmin) {
      this.loadTenants();
    }
  }

  constructor(
    private reservationReportService: ReservationReportService,
    private toastr: ToastrService,
    private tenantService: TenantService,
    public authService: AuthService,
    private fb: FormBuilder,
  ) {
    this.reportForm = this.createForm();
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
  }

  private createForm(): FormGroup {
    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;

    if (this.authService.hasRole('SuperAdmin')) {
      return this.fb.group(
        {
          dateFrom: [null, Validators.required],
          dateTo: [null, Validators.required],
          tenantId: [null, Validators.required]
        },
        { validators: this.dateRangeValidator }
      );
    } else {
      return this.fb.group(
        {
          dateFrom: [null, Validators.required],
          dateTo: [null, Validators.required]
        },
        { validators: this.dateRangeValidator }
      );
    }
  }

  dateRangeValidator(group: AbstractControl) {

    const from = group.get('dateFrom')?.value;
    const to = group.get('dateTo')?.value;

    if (!from || !to) return null;

    if (new Date(from) > new Date(to)) {
      return { invalidDateRange: true };
    }

    return null;
  }

  // Metoda za filtriranje po organizaciji
  applyTenantFilter(): void {
    if (this.selectedTenantFilter) {
      this.filteredReports = this.reports.filter(
        resurs => resurs.tenantId === this.selectedTenantFilter
      );
    } else {
      this.filteredReports = this.reports;
    }
  }

  onTenantFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedTenantFilter = value ? parseInt(value, 10) : null;
    this.applyTenantFilter();
  }

  getTenantName(tenantId: number): string {
    const tenant = this.tenants.find(t => t.id === tenantId);
    return tenant ? tenant.name : `Organizacija ${tenantId}`;
  }

  loadTenants(): void {
    this.loadingTenants = true;
    this.tenantService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
        this.loadingTenants = false;
      },
      error: (error) => {
        console.error('Greška pri učitavanju organizacija:', error);
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loadingTenants = false;
      }
    });
  }

  generateReport() {

    if (this.reportForm.invalid) return;

    const dto = this.reportForm.value;

    if(this.isAdmin){
      dto.tenantId=this.currentUserTenantId;
    }

    console.log(dto);

    this.reservationReportService.generateReport(dto).subscribe(() => {
      this.closeModal();
      this.loadReports();
    });
  }

  loadReports() {
    this.loading = true;
    if (this.isSuperAdmin) {
      this.reservationReportService.getReports().subscribe({
        next: (data) => {
          this.reports = data;
          this.applyTenantFilter();
          this.loading = false;
        },
        error: (error) => {
          console.error('Greška pri učitavanju izvještaja:', error);
          this.toastr.error('Greška pri učitavanju izvještaja');
          this.loading = false;
        }
      });
    } else if (this.isAdmin) {
      console.log(this.currentUserTenantId)
      if (this.currentUserTenantId) {
        this.reservationReportService.getReportsByTenantId(this.currentUserTenantId).subscribe({
          next: (data) => {
            this.reports = data;
            this.filteredReports= data;
            this.loading = false;
          },
          error: (error) => {
            console.error('Greška pri učitavanju izvještaja:', error);
            this.toastr.error('Greška pri učitavanju izvještaja');
            this.loading = false;
          }
        });
      }
    } else {
      this.loading = false;
      this.toastr.error('Nije pronađena organizacija korisnika');
    }
  }

  download(id: number, fileName: string) {
    this.reservationReportService.downloadReport(id).subscribe(blob => {
      const url = window.URL.createObjectURL(blob);

      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      a.click();

      window.URL.revokeObjectURL(url);
    });
  }

  formatDate(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('bs-BA');
  }

  // Getter za resurse koji se prikazuju (sa ili bez filtera)
  get displayReservationReports(): ReservationReport[] {
    return this.isSuperAdmin ? this.filteredReports : this.reports;
  }


  closeModal(): void {
    this.showModal = false;
    this.reportForm = this.createForm();
  }

  openCreateModal(): void {
    this.showModal = true;
  }

}
