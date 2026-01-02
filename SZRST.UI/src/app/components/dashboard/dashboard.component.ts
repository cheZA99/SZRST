import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TenantService, Tenant } from 'src/app/services/tenant.service';
import { FacilityService } from 'src/app/services/facility.service';
import { AuthService } from 'src/app/services/auth.service';
import { AppointmentService } from 'src/app/services/appointment.service';
import { UserService } from 'src/app/services/user.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit {
  isSuperAdmin = false;
  isAdmin = false;
  isUposlenik = false;
  isKorisnik = false;

  tenants: Tenant[] = [];
  facilities: any[] = [];
  filteredFacilities: any[] = [];

  stats = {
    totalUsers: 0,
    totalAppointmentsToday: 0,
    totalTenants: 0,
    totalFacilities: 0,
    activeAppointments: 0,
  };

  loading = false;
  loadingStats = false;

  currentUserTenantId: number | null = null;

  constructor(
    private router: Router,
    private toastr: ToastrService,
    private tenantService: TenantService,
    private facilityService: FacilityService,
    private authService: AuthService,
    private appointmentService: AppointmentService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.checkPermissions();
    this.loadDashboardData();
    this.loadStatistics();
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUposlenik = this.authService.hasRole('Uposlenik');
    this.isKorisnik = this.authService.hasRole('Korisnik');

    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;
  }

  loadDashboardData(): void {
    this.loading = true;

    this.facilityService.getAll().subscribe({
      next: (facilitiesData) => {
        this.facilities = facilitiesData;

        if (this.isSuperAdmin || this.isKorisnik) {
          this.loadTenants();
        } else if (this.isAdmin || this.isUposlenik) {
          this.loadFacilitiesByTenant();
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Greška pri učitavanju facilitija:', error);
        this.toastr.error('Greška pri učitavanju facilitija');
        this.loading = false;
      },
    });
  }

  loadTenants(): void {
    this.tenantService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
        this.stats.totalTenants = data.length;

        this.stats.totalFacilities = this.facilities.length;
        this.loading = false;
      },
      error: (error) => {
        console.error('Greška pri učitavanju organizacija:', error);
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loading = false;
      },
    });
  }

  loadFacilitiesByTenant(): void {
    if (!this.currentUserTenantId) {
      this.toastr.error('Nije pronađena organizacija korisnika');
      this.loading = false;
      return;
    }

    this.filteredFacilities = this.facilities.filter(
      (f) => f.tenantId === this.currentUserTenantId
    );
    this.stats.totalFacilities = this.filteredFacilities.length;
  }

  loadStatistics(): void {
    this.loadingStats = true;

    this.appointmentService.getDashboardStats().subscribe({
      next: (data) => {
        this.stats = {
          totalUsers: data.totalUsers || 0,
          totalAppointmentsToday: data.totalAppointmentsToday || 0,
          totalTenants: data.totalTenants || 0,
          totalFacilities: data.totalFacilities || 0,
          activeAppointments: data.activeAppointments || 0,
        };
        this.loadingStats = false;
      },
      error: (error) => {
        console.error('Greška pri učitavanju statistika:', error);
        this.stats = {
          totalUsers: 0,
          totalAppointmentsToday: 0,
          totalTenants: this.tenants.length,
          totalFacilities: this.facilities.length,
          activeAppointments: 0,
        };
        this.loadingStats = false;
      },
    });
  }

  openOrganizationCalendar(tenantId: number, tenantName: string): void {
    this.router.navigate(['/rezervacije'], {
      queryParams: {
        tenantId: tenantId,
        tenantName: encodeURIComponent(tenantName),
        autoSelect: 'true',
      },
    });
  }

  openFacilityCalendar(
    facilityId: number,
    facilityName: string,
    tenantId?: number
  ): void {
    const queryParams: any = {
      facilityId: facilityId,
      facilityName: encodeURIComponent(facilityName),
      autoSelect: 'true', 
    };

    if (tenantId) {
      queryParams.tenantId = tenantId;
      const tenant = this.tenants.find((t) => t.id === tenantId);
      if (tenant) {
        queryParams.tenantName = encodeURIComponent(tenant.name);
      }
    }

    this.router.navigate(['/rezervacije'], { queryParams });
  }

  getTenantFacilityCount(tenantId: number): number {
    if (!this.facilities.length) return 0;
    return this.facilities.filter((f) => f.tenantId === tenantId).length;
  }

  showOrganizations(): boolean {
    return this.isSuperAdmin || this.isKorisnik;
  }

  showFacilities(): boolean {
    return this.isAdmin || this.isUposlenik;
  }

  getWelcomeMessage(): string {
    const currentUser = this.authService.currentUser();
    const userName = currentUser?.userName || '';

    if (this.isSuperAdmin) {
      return `Dobrodošao/la ${userName}! Pregledajte organizacije i lokacije.`;
    } else if (this.isAdmin) {
      return `Dobrodošao/la ${userName}! Upravljajte rezervacijama vaše organizacije.`;
    } else if (this.isUposlenik) {
      return `Dobrodošao/la ${userName}! Pregledajte rezervacije.`;
    } else {
      return `Dobrodošao/la ${userName}! Rezervišite termin.`;
    }
  }
}
