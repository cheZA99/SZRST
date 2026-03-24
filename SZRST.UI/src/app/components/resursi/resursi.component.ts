import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { AppointmentTypeService, AppointmentType, AppointmentTypeCreateDto } from '../../services/appointment-type.service';
import { CurrencyService, Currency } from '../../services/currency.service';
import { TenantService, Tenant } from 'src/app/services/tenant.service';
import { AuthService } from '../../services/auth.service';
import { ConfirmDialogService } from 'src/app/services/confirm-dialog.service';
import { logger } from 'src/app/utils/logger';

@Component({
  selector: 'app-resursi',
  templateUrl: './resursi.component.html',
  styleUrls: ['./resursi.component.css']
})
export class ResursiComponent implements OnInit {
  appointmentTypes: AppointmentType[] = [];
  filteredAppointmentTypes: AppointmentType[] = [];
  currencies: Currency[] = [];
  tenants: Tenant[] = [];
  
  loading = false;
  loadingCurrencies = false;
  loadingTenants = false;
  
  showModal = false;
  isEditMode = false;
  selectedAppointmentType: AppointmentType | null = null;
  
  appointmentTypeForm: FormGroup;
  
  isSuperAdmin = false;
  isAdmin = false;
  isUposlenik = false;
  
  currentUserTenantId: number | null = null;
  
  selectedTenantFilter: number | null = null;

  constructor(
    private appointmentTypeService: AppointmentTypeService,
    private currencyService: CurrencyService,
    private tenantService: TenantService,
    public authService: AuthService,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private confirmDialog: ConfirmDialogService
  ) {
    this.appointmentTypeForm = this.createForm();
  }

  private createForm(): FormGroup {
    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;

    if (this.authService.hasRole('SuperAdmin')) {
      return this.fb.group({
        name: ['', [Validators.required, Validators.minLength(2)]],
        duration: [60, [Validators.required, Validators.min(15), Validators.max(480)]],
        price: [0, [Validators.required, Validators.min(0)]],
        currencyId: [null],
        tenantId: [null, Validators.required]
      });
    } else {
      return this.fb.group({
        name: ['', [Validators.required, Validators.minLength(2)]],
        duration: [60, [Validators.required, Validators.min(15), Validators.max(480)]],
        price: [0, [Validators.required, Validators.min(0)]],
        currencyId: [null]
      });
    }
  }

  get authServiceInstance(): AuthService {
    return this.authService;
  }

  ngOnInit(): void {
    this.checkPermissions();
    this.loadAppointmentTypes();
    this.loadCurrencies();

    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;

    if (this.isSuperAdmin) {
      this.loadTenants();
    }
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
    this.isUposlenik = this.authService.hasRole('Uposlenik');
  }

  loadAppointmentTypes(): void {
    this.loading = true;
    
    if (this.isSuperAdmin) {
      this.appointmentTypeService.getAll().subscribe({
        next: (data) => {
          this.appointmentTypes = data;
          this.applyTenantFilter();
          this.loading = false;
        },
        error: (error) => {
          logger.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju resursa:', error);
          this.toastr.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju resursa');
          this.loading = false;
        }
      });
    } else if (this.isAdmin || this.isUposlenik) {
      if (this.currentUserTenantId) {
        this.appointmentTypeService.getByTenant(this.currentUserTenantId).subscribe({
          next: (data) => {
            this.appointmentTypes = data;
            this.filteredAppointmentTypes = data; 
            this.loading = false;
          },
          error: (error) => {
            logger.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju resursa:', error);
            this.toastr.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju resursa');
            this.loading = false;
          }
        });
      } else {
        this.loading = false;
        this.toastr.error('Nije pronaÃƒâ€žÃ¢â‚¬Ëœena organizacija korisnika');
      }
    }
  }

  loadCurrencies(): void {
    this.loadingCurrencies = true;
    this.currencyService.getAll().subscribe({
      next: (data) => {
        this.currencies = data;
        this.loadingCurrencies = false;
      },
      error: (error) => {
        logger.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju valuta:', error);
        this.toastr.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju valuta');
        this.loadingCurrencies = false;
      }
    });
  }

  loadTenants(): void {
    this.loadingTenants = true;
    this.tenantService.getAllTenants().subscribe({
      next: (data) => {
        this.tenants = data;
        this.loadingTenants = false;
      },
      error: (error) => {
        logger.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju organizacija:', error);
        this.toastr.error('GreÃƒâ€¦Ã‚Â¡ka pri uÃƒâ€žÃ‚Âitavanju organizacija');
        this.loadingTenants = false;
      }
    });
  }

  applyTenantFilter(): void {
    if (this.selectedTenantFilter) {
      this.filteredAppointmentTypes = this.appointmentTypes.filter(
        resurs => resurs.tenantId === this.selectedTenantFilter
      );
    } else {
      this.filteredAppointmentTypes = this.appointmentTypes;
    }
  }

  onTenantFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedTenantFilter = value ? parseInt(value, 10) : null;
    this.applyTenantFilter();
  }

  clearFilter(): void {
    this.selectedTenantFilter = null;
    this.applyTenantFilter();
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.selectedAppointmentType = null;

    if (this.isSuperAdmin) {
      this.appointmentTypeForm.reset({
        duration: 60,
        price: 0,
        currencyId: null,
        tenantId: null
      });
    } else {
      this.appointmentTypeForm.reset({
        duration: 60,
        price: 0,
        currencyId: null
      });
    }

    this.showModal = true;
  }

  openEditModal(appointmentType: AppointmentType): void {
    this.isEditMode = true;
    this.selectedAppointmentType = appointmentType;

    if (this.isSuperAdmin) {
      this.appointmentTypeForm = this.fb.group({
        name: [appointmentType.name, [Validators.required, Validators.minLength(2)]],
        duration: [appointmentType.duration, [Validators.required, Validators.min(15), Validators.max(480)]],
        price: [appointmentType.price, [Validators.required, Validators.min(0)]],
        currencyId: [appointmentType.currencyId],
        tenantId: [appointmentType.tenantId, Validators.required]
      });
    } else {
      this.appointmentTypeForm = this.fb.group({
        name: [appointmentType.name, [Validators.required, Validators.minLength(2)]],
        duration: [appointmentType.duration, [Validators.required, Validators.min(15), Validators.max(480)]],
        price: [appointmentType.price, [Validators.required, Validators.min(0)]],
        currencyId: [appointmentType.currencyId]
      });
    }

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.appointmentTypeForm = this.createForm();
    this.selectedAppointmentType = null;
  }

  onSubmit(): void {
    if (this.appointmentTypeForm.invalid) {
      Object.keys(this.appointmentTypeForm.controls).forEach((key) => {
        this.appointmentTypeForm.get(key)?.markAsTouched();
      });
      this.toastr.error('Molimo ispravite greÃƒâ€¦Ã‚Â¡ke u formi');
      return;
    }

    const formData = this.appointmentTypeForm.value;

    if (this.isAdmin && !this.isSuperAdmin) {
      formData.tenantId = this.currentUserTenantId;
    }

    if (this.isEditMode && this.selectedAppointmentType) {
      this.updateAppointmentType(this.selectedAppointmentType.id, formData);
    } else {
      this.createAppointmentType(formData);
    }
  }

  createAppointmentType(data: AppointmentTypeCreateDto): void {
    if (this.isSuperAdmin && !data.tenantId) {
      this.toastr.error('Morate odabrati organizaciju');
      return;
    }

    if (this.isAdmin && !this.isSuperAdmin && !data.tenantId) {
      this.toastr.error('Nije pronaÃƒâ€žÃ¢â‚¬Ëœena organizacija');
      return;
    }

    this.appointmentTypeService.create(data).subscribe({
      next: () => {
        this.toastr.success('Resurs uspjeÃƒâ€¦Ã‚Â¡no kreiran');
        this.loadAppointmentTypes();
        this.closeModal();
      },
      error: (error) => {
        logger.error('GreÃƒâ€¦Ã‚Â¡ka pri kreiranju resursa:', error);
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => {
            this.toastr.error(err);
          });
        } else {
          this.toastr.error('GreÃƒâ€¦Ã‚Â¡ka pri kreiranju resursa');
        }
      }
    });
  }

  updateAppointmentType(id: number, data: AppointmentTypeCreateDto): void {
    if (this.isAdmin && !this.isSuperAdmin && this.selectedAppointmentType?.tenantId) {
      data.tenantId = this.selectedAppointmentType.tenantId;
    }

    this.appointmentTypeService.update(id, data).subscribe({
      next: () => {
        this.toastr.success('Resurs uspjeÃƒâ€¦Ã‚Â¡no aÃƒâ€¦Ã‚Â¾uriran');
        this.loadAppointmentTypes();
        this.closeModal();
      },
      error: (error) => {
        logger.error('GreÃƒâ€¦Ã‚Â¡ka pri aÃƒâ€¦Ã‚Â¾uriranju resursa:', error);
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => {
            this.toastr.error(err);
          });
        } else {
          this.toastr.error('GreÃƒâ€¦Ã‚Â¡ka pri aÃƒâ€¦Ã‚Â¾uriranju resursa');
        }
      }
    });
  }  async deleteAppointmentType(id: number): Promise<void> {
    const confirmed = await this.confirmDialog.confirm({
      title: 'Potvrda brisanja',
      text: 'Da li ste sigurni da Å¾elite obrisati ovaj resurs?',
      confirmButtonText: 'ObriÅ¡i',
      cancelButtonText: 'OtkaÅ¾i'
    });

    if (!confirmed) {
      return;
    }

    this.appointmentTypeService.delete(id).subscribe({
      next: () => {
        this.toastr.success('Resurs uspjeÅ¡no obrisan');
        this.loadAppointmentTypes();
      },
      error: (error) => {
        logger.error('GreÅ¡ka pri brisanju resursa:', error);
        this.toastr.error('GreÅ¡ka pri brisanju resursa');
      }
    });
  }

  formatDuration(minutes: number): string {
    if (minutes < 60) {
      return `${minutes} min`;
    } else if (minutes === 60) {
      return '1 sat';
    } else if (minutes % 60 === 0) {
      return `${minutes / 60} sata`;
    } else {
      const hours = Math.floor(minutes / 60);
      const mins = minutes % 60;
      return `${hours}h ${mins}min`;
    }
  }

  formatPrice(price: number, currencyName?: string): string {
    if (currencyName) {
      return `${price.toFixed(2)} ${currencyName}`;
    }
    return `${price.toFixed(2)}`;
  }

  formatDate(date: Date | string): string {
    if (!date) return '';
    const d = new Date(date);
    return d.toLocaleDateString('bs-BA');
  }

  getTenantName(tenantId: number | null): string {
    if (tenantId === null) {
      return '';
    }

    const tenant = this.tenants.find(t => t.id === tenantId);
    return tenant ? tenant.name : `Organizacija ${tenantId}`;
  }

  get displayAppointmentTypes(): AppointmentType[] {
    return this.isSuperAdmin ? this.filteredAppointmentTypes : this.appointmentTypes;
  }
}
