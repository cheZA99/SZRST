import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import {
  FacilityService,
  FacilityResponse,
  Tenant
} from '../../services/facility.service';
import { AuthService } from '../../services/auth.service';
import { TenantService } from 'src/app/services/tenant.service';
import { FacilityTypeService } from 'src/app/services/facility-type.service'
import { FacilityType } from 'src/app/services/facility-type.service';
import { Country, CountryService } from 'src/app/services/country.service';
import { City, CityService } from 'src/app/services/city.service';
import { environment } from 'src/environments/environment';
import { TranslateService } from '@ngx-translate/core';
import Swal from 'sweetalert2';

import {
  trigger,
  transition,
  style,
  animate
} from '@angular/animations';


@Component({
  selector: 'app-lokacije-temp',
  templateUrl: './lokacije.component.html',
  styleUrls: ['./lokacije.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(10px)' }),
        animate('300ms ease-out',
          style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ]),
    trigger('modalAnimation', [
      transition(':enter', [
        style({ opacity: 0, transform: 'scale(0.9)' }),
        animate('200ms ease-out',
          style({ opacity: 1, transform: 'scale(1)' }))
      ]),
      transition(':leave', [
        animate('150ms ease-in',
          style({ opacity: 0, transform: 'scale(0.9)' }))
      ])
    ])
  ]
})
export class LokacijeComponent implements OnInit {

  private translate = inject(TranslateService);

  facilities: FacilityResponse[] = [];
  filteredFacilities: FacilityResponse[] = [];
  tenants: Tenant[] = [];
  facilityTypes: FacilityType[] = [];
  countries: Country[] = [];
  cities: City[] = [];
  filteredCities: City[] = [];
  filteredCitiesSearch: City[] = [];

  loading = false;
  loadingTenants = false;

  showModal = false;
  showDeleteModal = false;
  isEditMode = false;
  selectedFacility: FacilityResponse | null = null;
  selectedFile: File | null = null;
  imagePreview: string | null = null;
  removeCurrentImage = false;

  facilityForm!: FormGroup;
  filterForm!: FormGroup;

  isSuperAdmin = false;
  isAdmin = false;

  currentUserTenantId: number | null = null;

  selectedTenantFilter: number | null = null;

  currentPage = 1;
  pageSize = 5;
  totalCount = 0;
  totalPages = 0;
  sortColumn: string = 'name';
  sortDirection: 'asc' | 'desc' = 'asc';

  Math = Math;

  constructor(
    private facilityService: FacilityService,
    private tenantService: TenantService,
    private facilityTypeService: FacilityTypeService,
    private countryService: CountryService,
    private cityService: CityService,
    public authService: AuthService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {
    this.facilityForm = this.createForm();
  }

  buildFilterForm(): void {
    this.filterForm = this.fb.group({
      name: [''],
      facilityTypeId: [null],
      address: [''],
      countryId: [null],
      cityId: [null],
      tenantId: [null],
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadFacilities();
  }

  clearFilter(): void {
    this.filterForm.reset({
      userName: '',
      facilityTypeId: null,
      address: '',
      countryId: null,
      cityId: null,
      tenantId: null,
    });
    this.currentPage = 1;
    this.loadFacilities();
  }

  private createForm(): FormGroup {
    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;

    if (this.authService.hasRole('SuperAdmin')) {
      return this.fb.group(
        {
          name: ['', [Validators.required, Validators.minLength(3)]],
          facilityTypeId: [null, [Validators.required]],
          imageUrl: [''],
          address: ['', [Validators.required, Validators.minLength(3)]],
          addressNumber: ['', [Validators.required]],
          countryId: [null, [Validators.required]],
          cityId: [null, [Validators.required]],
          tenantId: [null, Validators.required],
        }
      );
    } else {
      return this.fb.group(
        {
          name: ['', [Validators.required, Validators.minLength(3)]],
          facilityTypeId: [null, [Validators.required]],
          imageUrl: [''],
          address: ['', [Validators.required, Validators.minLength(3)]],
          addressNumber: ['', [Validators.required]],
          countryId: [null, [Validators.required]],
          cityId: [null, [Validators.required]],
        }
      );
    }
  }

  get authServiceInstance(): AuthService {
    return this.authService;
  }

  ngOnInit(): void {
    const savedLang = localStorage.getItem('lang') || 'bs';
    this.translate.use(savedLang);

    this.buildFilterForm();

    this.checkPermissions();
    this.loadFacilities();

    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;
    this.loadFacilityTypes();
    this.loadCountries();
    this.loadCities();

    if (this.isSuperAdmin) {
      this.loadTenants();
    }
  }

  ngOnDestroy() {
    if (this.imagePreview) {
      URL.revokeObjectURL(this.imagePreview);
    }
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
  }

  loadFacilities(): void {
    this.loading = true;

    const fv = this.filterForm.value;

    this.facilityService
      .getAll({
        pageNumber: this.currentPage,
        pageSize: this.pageSize,
        name: fv.name || undefined,
        facilityTypeId: fv.facilityTypeId || undefined,
        address: fv.address || undefined,
        countryId: fv.countryId || undefined,
        cityId: fv.cityId || undefined,
        tenantId: fv.tenantId || undefined,
        sortColumn: this.sortColumn,
        sortDirection: this.sortDirection
      })
      .subscribe({
        next: (result) => {
          console.log(result.items)
          this.facilities = result.items;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.currentPage = result.pageNumber;
          this.loading = false;
        },
        error: (error) => {
          console.error('Greška pri učitavanju objekata:', error);
          this.toastr.error('Greška pri učitavanju objekata');
          this.loading = false;
        },
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
        console.error('Greška pri učitavanju organizacija:', error);
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loadingTenants = false;
      },
    });
  }

  loadFacilityTypes(): void {
    this.facilityTypeService.getAll().subscribe({
      next: (data) => {
        this.facilityTypes = data;
      },
      error: (error) => {
        console.error('Greška pri učitavanju facility types:', error);
        this.toastr.error('Greška pri učitavanju facility types');
        this.loadingTenants = false;
      }
    })
  }

  sortBy(column: string) {

    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }

    this.loadFacilities();
  }

  get visiblePages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  onPageSizeSelectChange(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1;
    this.loadFacilities();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.loadFacilities();
  }

  loadCities(): void {
    this.cityService.getCities().subscribe({
      next: (data) => {
        this.cities = data;
      },
      error: (error) => {
        console.error('Greška pri učitavanju gradova:', error);
        this.toastr.error('Greška pri učitavanju gradova');
        this.loadingTenants = false;
      }
    })
  }

  loadCountries(): void {
    this.countryService.getCountries().subscribe({
      next: (data) => {
        this.countries = data;
      },
      error: (error) => {
        console.error('Greška pri učitavanju država:', error);
        this.toastr.error('Greška pri učitavanju država');
        this.loadingTenants = false;
      }
    })
  }

  applyTenantFilter(): void {
    if (this.selectedTenantFilter) {
      this.filteredFacilities = this.facilities.filter(
        (emp) => emp.tenantId === this.selectedTenantFilter
      );
    } else {
      this.filteredFacilities = this.facilities;
    }
  }

  onTenantFilterChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    const value = select.value;
    this.selectedTenantFilter = value ? parseInt(value, 10) : null;
    this.applyTenantFilter();
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.selectedFacility = null;

    if (this.isSuperAdmin) {
      this.facilityForm.reset({
        active: true,
        tenantId: null,
      });
    } else if (this.isAdmin) {
      this.facilityForm.reset({
        active: true,
      });
    }

    this.showModal = true;
  }



  openEditModal(facility: FacilityResponse): void {
    this.filteredCities = this.cities.filter(x => x.country.id === facility.location.city.country.id);

    this.imagePreview = facility.imageUrl
      ? `${environment.apiUrl}${facility.imageUrl}`
      : null;

    this.isEditMode = true;
    this.selectedFacility = facility;

    if (this.isSuperAdmin) {
      this.facilityForm = this.fb.group({
        name: [
          facility.name,
          [Validators.required, Validators.minLength(3)]
        ],
        facilityTypeId: [
          facility.facilityType.id,
          [Validators.required]
        ],
        imageUrl: [facility.imageUrl],
        address: [
          facility.location.address,
          [Validators.required, Validators.minLength(3)]
        ],
        addressNumber: [
          facility.location.addressNumber,
          [Validators.required]
        ],
        countryId: [
          facility.location.city.country.id,
          [Validators.required]
        ],
        cityId: [
          facility.location.city.id,
          [Validators.required]
        ],
        tenantId: [
          facility.tenantId,
          [Validators.required]
        ],
        locationId: [facility.location.id]
      });
    } else {
      this.facilityForm = this.fb.group({
        name: [
          facility.name,
          [Validators.required, Validators.minLength(3)]
        ],
        facilityTypeId: [
          facility.facilityType.id,
          [Validators.required]
        ],
        imageUrl: [facility.imageUrl],
        address: [
          facility.location.address,
          [Validators.required, Validators.minLength(3)]
        ],
        addressNumber: [
          facility.location.addressNumber,
          [Validators.required]
        ],
        countryId: [
          facility.location.city.country.id,
          [Validators.required]
        ],
        cityId: [
          facility.location.city.id,
          [Validators.required]
        ],
        locationId: [facility.location.id]
      });
    }

    this.showModal = true;
  }

  onCountryChange(): void {
    const cityControl = this.facilityForm.get('cityId');

    cityControl?.reset();
    cityControl?.markAsTouched();
    cityControl?.markAsDirty();
    cityControl?.updateValueAndValidity();
  }

  closeModal(): void {
    this.showModal = false;

    this.facilityForm = this.createForm();
    this.selectedFacility = null;

    this.selectedFile = null;
    this.imagePreview = null;
  }

  onSubmit(): void {
    if (this.facilityForm.invalid) {
      Object.keys(this.facilityForm.controls).forEach((key) => {
        this.facilityForm.get(key)?.markAsTouched();
      });
      return;
    }

    const formData = this.facilityForm.value;

    if (this.isAdmin && !this.isSuperAdmin) {
      formData.tenantId = this.currentUserTenantId;
    }

    if (this.isEditMode && this.selectedFacility) {
      this.updateFacility(this.selectedFacility.id, formData);
    } else {
      this.createFacility(formData);
    }
  }

confirmDeleteFacility(id: number) {

  const title = this.translate.instant('FACILITY.DELETE_CONFIRM_TITLE');
  const text = this.translate.instant('FACILITY.DELETE_CONFIRM_TEXT');
  const confirmBtn = this.translate.instant('FACILITY.DELETE_CONFIRM_BUTTON');
  const cancelBtn = this.translate.instant('FACILITY.CANCEL');

  Swal.fire({
    title: title,
    text: text,
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: confirmBtn,
    cancelButtonText: cancelBtn,
    confirmButtonColor: '#d33'
  }).then((result) => {
    if (result.isConfirmed) {
      this.deleteFacility(id);
    }
  });
}

  deleteFacility(id: number) {
    this.facilityService.delete(id).subscribe({
      next: (response) => {
        this.toastr.success('Facility uspješno izbrisan');
        this.loadFacilities();
        this.closeModal();
      },
      error: (error) => {
        console.error('Greška pri brisanju facilitija:', error);
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => {
            this.toastr.error(err);
          });
        } else {
          this.toastr.error('Greška pri brisanju facilitija');
        }
      },
    })
  }

  createFacility(data: any): void {
    console.log("Dataa--->", data);
    if (this.isSuperAdmin && !data.tenantId) {
      this.toastr.error('Morate odabrati organizaciju');
      return;
    }

    if (this.isAdmin && !this.isSuperAdmin) {
      data.tenantId = this.currentUserTenantId;
    }

    this.facilityService.createWithLocation(data, this.selectedFile).subscribe({
      next: (response) => {
        this.toastr.success('Facility uspješno kreiran');
        this.loadFacilities();
        this.closeModal();
      },
      error: (error) => {
        console.error('Greška pri kreiranju facilitija:', error);
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => {
            this.toastr.error(err);
          });
        } else {
          this.toastr.error('Greška pri kreiranju facilitija');
        }
      },
    });
  }


  onFileSelected(event: Event) {

    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    this.selectedFile = file;

    if (this.imagePreview) {
      URL.revokeObjectURL(this.imagePreview);
    }

    this.imagePreview = URL.createObjectURL(file);
  }

  removeImage() {

    if (this.imagePreview) {
      URL.revokeObjectURL(this.imagePreview);
    }

    this.imagePreview = null;
    this.selectedFile = null;
    this.removeCurrentImage = true;
  }

  filterCities(): void {
    this.onCountryChange();
    const countryId = Number(
      this.facilityForm.get('countryId')?.value
    );

    this.filteredCities = this.cities.filter(x => x.country.id === countryId);

    console.log(this.filteredCities);
  }

  filterCitiesSearch(): void {
    this.onCountryChange();
    const countryId = Number(
      this.filterForm.get('countryId')?.value
    );

    this.filteredCitiesSearch = this.cities.filter(x => x.country.id === countryId);

  }

  updateFacility(id: number, data: any): void {
    if (this.isAdmin && !this.isSuperAdmin) {
      data.tenantId = this.selectedFacility?.tenantId;
    }

    this.facilityService.updateWithLocation(id, data, this.selectedFile).subscribe({
      next: () => {
        this.toastr.success('Lokacija uspješno ažurirana');
        this.loadFacilities();
        this.closeModal();
      },
      error: (error) => {
        console.error('Greška pri ažuriranju lokacije:', error);
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => {
            this.toastr.error(err);
          });
        } else {
          this.toastr.error('Greška pri ažuriranju lokacije');
        }
      },
    });
  }

  getTenantName(tenantId: number | null): string {
    if (tenantId === null) return '';
    const tenant = this.tenants.find((t) => t.id === tenantId);
    return tenant ? tenant.name : `Organizacija ${tenantId}`;
  }

  get displayFacilities(): FacilityResponse[] {
    return this.isSuperAdmin ? this.filteredFacilities : this.facilities;
  }
}
