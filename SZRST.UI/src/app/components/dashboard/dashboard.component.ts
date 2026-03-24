import { Component, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TenantService, Tenant } from 'src/app/services/tenant.service';
import { FacilityService } from 'src/app/services/facility.service';
import { AuthService } from 'src/app/services/auth.service';
import { AppointmentService } from 'src/app/services/appointment.service';
import { environment } from 'src/environments/environment';
import { Subject, interval } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { logger } from 'src/app/utils/logger';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css'],
})
export class DashboardComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly statsRefreshMs = 10000;
  isSuperAdmin = false;
  isAdmin = false;
  isUposlenik = false;
  isKorisnik = false;

  tenants: Tenant[] = [];
  facilities: any[] = [];
  filteredFacilities: any[] = [];

  selectedTenantId: number | null = null;
  selectedFacilityId: number | null = null;
  selectedTenantName: string = '';
  selectedTenantFacilities: any[] = [];
  apiUrl: string = environment.apiUrl;

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
  currentUserTenantName: string = '';

  constructor(
    private router: Router,
    private toastr: ToastrService,
    private tenantService: TenantService,
    private facilityService: FacilityService,
    private authService: AuthService,
    private appointmentService: AppointmentService
  ) {}

  ngOnInit(): void {
    this.checkPermissions();
    this.loadDashboardData();
    this.loadStatistics();
    this.startAutoRefresh();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUposlenik = this.authService.hasRole('Uposlenik');
    this.isKorisnik = this.authService.hasRole('Korisnik');

    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;
    
    if ((this.isAdmin || this.isUposlenik) && this.currentUserTenantId) {
      this.loadTenantName();
    }
  }

  loadTenantName(): void {
    if (!this.currentUserTenantId) return;
    
    this.tenantService.getTenantById(this.currentUserTenantId).subscribe({
      next: (tenant) => {
        this.currentUserTenantName = tenant.name;
      },
      error: (error) => {
        logger.error('Greška pri učitavanju organizacije:', error);
        this.currentUserTenantName = '';
      }
    });
  }

  loadDashboardData(): void {
    this.loading = true;

    this.facilityService.getAll().subscribe({
      next: (facilitiesData) => {
        this.facilities = facilitiesData.items;

        if (this.isSuperAdmin || this.isKorisnik) {
          this.loadTenants();
        } else if (this.isAdmin || this.isUposlenik) {
          this.filteredFacilities = facilitiesData.items.filter(
            (f) => f.tenantId === this.currentUserTenantId
          );
          this.stats.totalFacilities = this.filteredFacilities.length;
          this.loading = false;
        }
      },
      error: (error) => {
        logger.error('Greška pri učitavanju facilitija:', error);
        this.toastr.error('Greška pri učitavanju facilitija');
        this.loading = false;
      },
    });
  }

  loadTenants(): void {
    this.tenantService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
        
        if (data.length > 0 && !this.selectedTenantId) {
          this.selectTenant(data[0]);
        }
        
        this.loading = false;
      },
      error: (error) => {
        logger.error('Greška pri učitavanju organizacija:', error);
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loading = false;
      },
    });
  }

  loadStatistics(): void {
    this.loadingStats = true;

    if (this.isKorisnik) {
      this.stats = {
        totalUsers: 0,
        totalAppointmentsToday: 0,
        totalTenants: this.tenants.length,
        totalFacilities: this.facilities.length,
        activeAppointments: 0,
      };
      this.loadingStats = false;
      return;
    }

    let tenantIdForStats = null;
    
    if (this.isAdmin || this.isUposlenik) {
      tenantIdForStats = this.currentUserTenantId;
    }

    this.appointmentService.getDashboardStats(tenantIdForStats).subscribe({
      next: (data) => {
        this.stats = {
          totalUsers: data.totalUsers || 0,
          totalAppointmentsToday: data.totalAppointmentsToday || 0,
          totalTenants: data.totalTenants || 0,
          totalFacilities: data.totalFacilities || 0,
          activeAppointments: data.activeAppointments || 0,
        };
        
        if (this.isAdmin || this.isUposlenik) {
          this.stats.totalTenants = 0;
        }
        
        this.loadingStats = false;
      },
      error: (error) => {
        logger.error('Greška pri učitavanju statistika:', error);
        this.stats = {
          totalUsers: 0,
          totalAppointmentsToday: 0,
          totalTenants: this.isAdmin || this.isUposlenik ? 0 : this.tenants.length,
          totalFacilities: this.isAdmin || this.isUposlenik 
            ? this.filteredFacilities.length 
            : this.facilities.length,
          activeAppointments: 0,
        };
        this.loadingStats = false;
      },
    });
  }

  private startAutoRefresh(): void {
    interval(this.statsRefreshMs)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.loadStatistics();
      });
  }

selectTenant(tenant: Tenant): void {
  this.selectedTenantId = tenant.id;
  this.selectedTenantName = tenant.name;
  
  this.selectedTenantFacilities = this.facilities.filter(
    (f) => f.tenantId === tenant.id
  );
  
  this.selectedFacilityId = null;
}

openFacilityCalendar(facility: any): void {
  this.selectedFacilityId = facility.id;
  
  let tenantIdForFacility = facility.tenantId;
  let tenantNameForFacility = '';
  
  if (this.isSuperAdmin || this.isKorisnik) {
    const tenant = this.tenants.find(t => t.id === facility.tenantId);
    if (tenant) {
      tenantIdForFacility = tenant.id;
      tenantNameForFacility = tenant.name;
    }
  } else if (this.isAdmin || this.isUposlenik) {
    tenantIdForFacility = this.currentUserTenantId;
    tenantNameForFacility = this.currentUserTenantName;
  }

  const queryParams: any = {
    facilityId: facility.id,
    facilityName: encodeURIComponent(facility.name),
    tenantId: tenantIdForFacility,
    tenantName: encodeURIComponent(tenantNameForFacility),
    autoSelect: 'true',
    fromDashboard: 'true'
  };

  logger.log('Navigating to reservations with:', queryParams);
  
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
      return `Dobrodošao/la ${userName}!`;
    } else if (this.isUposlenik) {
      return `Dobrodošao/la ${userName}!`;
    } else {
      return `Dobrodošao/la ${userName}! Rezervišite termin.`;
    }
  }

  shareFacebook() {
  const url = encodeURIComponent(window.location.href);
  window.open(`https://www.facebook.com/sharer/sharer.php?u=${url}`, '_blank');
}

shareTwitter() {
  const url = encodeURIComponent(window.location.href);
  const text = encodeURIComponent("Pogledajte ovu objavu!");
  window.open(`https://twitter.com/intent/tweet?url=${url}&text=${text}`, '_blank');
}

shareLinkedIn() {
  const url = encodeURIComponent(window.location.href);
  window.open(`https://www.linkedin.com/sharing/share-offsite/?url=${url}`, '_blank');
}

  getFacilityLocationLabel(facility: any): string {
    const address = facility?.location?.address;
    const addressNumber = facility?.location?.addressNumber;
    const cityName = facility?.location?.city?.name;
    const countryName =
      facility?.location?.city?.country?.name ?? facility?.location?.country?.name;

    const parts = [address, addressNumber, cityName, countryName]
      .filter((part) => !!part)
      .map((part) => `${part}`.trim())
      .filter((part) => part.length > 0);

    return parts.length > 0 ? parts.join(', ') : 'Lokacija nije definirana';
  }
  
}
