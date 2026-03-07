import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import {
  UposleniciService,
  Employee,
  Tenant,
} from '../../services/uposlenici.service';
import { AuthService } from '../../services/auth.service';
import { TenantService } from 'src/app/services/tenant.service';

@Component({
  selector: 'app-uposlenici',
  templateUrl: './uposlenici.component.html',
  styleUrls: ['./uposlenici.component.css'],
})
export class UposleniciComponent implements OnInit {
  employees: Employee[] = [];
  tenants: Tenant[] = [];

  loading = false;
  loadingTenants = false;

  showModal = false;
  isEditMode = false;
  selectedEmployee: Employee | null = null;

  employeeForm!: FormGroup;
  filterForm!: FormGroup;

  showPassword = false;
  showConfirmPassword = false;
  showNewPassword = false;
  showNewConfirmPassword = false;

  isSuperAdmin = false;
  isAdmin = false;

  currentUserTenantId: number | null = null;

  currentPage = 1;
  pageSize = 5;
  totalCount = 0;
  totalPages = 0;

  Math = Math;

  constructor(
    private uposleniciService: UposleniciService,
    private tenantService: TenantService,
    public authService: AuthService,
    private fb: FormBuilder,
    private toastr: ToastrService,
  ) {}

  ngOnInit(): void {
    this.checkPermissions();
    this.buildFilterForm();
    this.buildEmployeeForm();
    this.loadEmployees();

    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;

    if (this.isSuperAdmin) {
      this.loadTenants();
    }
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');
  }

  buildFilterForm(): void {
    this.filterForm = this.fb.group({
      userName: [''],
      email: [''],
      firstName: [''],
      lastName: [''],
      tenantId: [null],
    });
  }

  applyFilter(): void {
    this.currentPage = 1;
    this.loadEmployees();
  }

  clearFilter(): void {
    this.filterForm.reset({
      userName: '',
      email: '',
      firstName: '',
      lastName: '',
      tenantId: null,
    });
    this.currentPage = 1;
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading = true;
    const fv = this.filterForm.value;

    this.uposleniciService
      .getEmployees({
        pageNumber: this.currentPage,
        pageSize: this.pageSize,
        userName: fv.userName || undefined,
        email: fv.email || undefined,
        firstName: fv.firstName || undefined,
        lastName: fv.lastName || undefined,
        tenantId: fv.tenantId || undefined,
      })
      .subscribe({
        next: (result) => {
          this.employees = result.items;
          this.totalCount = result.totalCount;
          this.totalPages = result.totalPages;
          this.currentPage = result.pageNumber;
          this.loading = false;
        },
        error: (error) => {
          console.error('Greška pri učitavanju uposlenika:', error);
          this.toastr.error('Greška pri učitavanju uposlenika');
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
      error: () => {
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loadingTenants = false;
      },
    });
  }

  get visiblePages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.loadEmployees();
  }

  onPageSizeSelectChange(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1;
    this.loadEmployees();
  }

  getTenantName(tenantId: number): string {
    const tenant = this.tenants.find((t) => t.id === tenantId);
    return tenant ? tenant.name : `Organizacija ${tenantId}`;
  }

  buildEmployeeForm(emp?: Employee): void {
    if (this.isEditMode && emp) {
      const fields: any = {
        userName: [
          emp.userName,
          [Validators.required, Validators.minLength(3)],
        ],
        email: [emp.email, [Validators.required, Validators.email]],
        firstName: [emp.firstName ?? ''],
        lastName: [emp.lastName ?? ''],
        active: [emp.active],
        newPassword: [''],
        newConfirmPassword: [''],
      };

      if (this.isSuperAdmin) {
        fields['tenantId'] = [emp.tenantId, Validators.required];
      }

      this.employeeForm = this.fb.group(fields, {
        validators: this.passwordMatchValidator,
      });
    } else {
      const fields: any = {
        userName: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        firstName: [''],
        lastName: [''],
        active: [true],
        password: ['', [Validators.required, Validators.minLength(6)]],
        confirmPassword: ['', Validators.required],
        newPassword: [''],
        newConfirmPassword: [''],
      };

      if (this.isSuperAdmin) {
        fields['tenantId'] = [null, Validators.required];
      }

      this.employeeForm = this.fb.group(fields, {
        validators: this.passwordMatchValidator,
      });
    }
  }

  passwordMatchValidator(g: FormGroup) {
    const errors: any = {};

    const pw = g.get('password')?.value;
    const confirmPw = g.get('confirmPassword')?.value;
    if (pw && pw !== confirmPw) errors['passwordMismatch'] = true;

    const newPw = g.get('newPassword')?.value;
    const confirmNewPw = g.get('newConfirmPassword')?.value;
    if (newPw && newPw !== confirmNewPw) errors['newPasswordMismatch'] = true;

    return Object.keys(errors).length > 0 ? errors : null;
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.selectedEmployee = null;
    this.buildEmployeeForm();
    this.showModal = true;
  }

  openEditModal(employee: Employee): void {
    this.isEditMode = true;
    this.selectedEmployee = employee;
    this.buildEmployeeForm(employee);
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showPassword = false;
    this.showConfirmPassword = false;
    this.showNewPassword = false;
    this.showNewConfirmPassword = false;
    this.selectedEmployee = null;
    this.isEditMode = false;
    this.buildEmployeeForm();
  }

  togglePasswordVisibility(
    field: 'password' | 'confirm' | 'newPassword' | 'newConfirm',
  ): void {
    switch (field) {
      case 'password':
        this.showPassword = !this.showPassword;
        break;
      case 'confirm':
        this.showConfirmPassword = !this.showConfirmPassword;
        break;
      case 'newPassword':
        this.showNewPassword = !this.showNewPassword;
        break;
      case 'newConfirm':
        this.showNewConfirmPassword = !this.showNewConfirmPassword;
        break;
    }
  }

  onSubmit(): void {
    if (this.employeeForm.invalid) {
      Object.keys(this.employeeForm.controls).forEach((key) =>
        this.employeeForm.get(key)?.markAsTouched(),
      );

      if (this.employeeForm.errors?.['passwordMismatch'])
        this.toastr.error('Lozinke se ne podudaraju');
      if (this.employeeForm.errors?.['newPasswordMismatch'])
        this.toastr.error('Nove lozinke se ne podudaraju');

      return;
    }

    const formData = { ...this.employeeForm.value };

    if (this.isAdmin && !this.isSuperAdmin) {
      formData.tenantId = this.currentUserTenantId;
    }

    if (this.isEditMode && this.selectedEmployee) {
      this.updateEmployee(this.selectedEmployee.id, formData);
    } else {
      this.createEmployee(formData);
    }
  }

  createEmployee(data: any): void {
    if (this.isSuperAdmin && !data.tenantId) {
      this.toastr.error('Morate odabrati organizaciju');
      return;
    }

    const dto = {
      userName: data.userName,
      email: data.email,
      firstName: data.firstName || null,
      lastName: data.lastName || null,
      password: data.password,
      confirmPassword: data.confirmPassword,
      tenantId: data.tenantId,
    };

    this.uposleniciService.createEmployee(dto).subscribe({
      next: () => {
        this.toastr.success('Uposlenik uspješno kreiran');
        this.loadEmployees();
        this.closeModal();
      },
      error: (error) => {
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => this.toastr.error(err));
        } else {
          this.toastr.error('Greška pri kreiranju uposlenika');
        }
      },
    });
  }

  updateEmployee(id: number, data: any): void {
    const dto: any = {
      userName: data.userName,
      email: data.email,
      firstName: data.firstName || null,
      lastName: data.lastName || null,
      active: data.active,
      tenantId: this.isSuperAdmin
        ? data.tenantId
        : this.selectedEmployee?.tenantId,
    };

    if (data.newPassword) {
      dto.newPassword = data.newPassword;
      dto.confirmPassword = data.newConfirmPassword;
    }

    this.uposleniciService.updateEmployee(id, dto).subscribe({
      next: () => {
        this.toastr.success('Uposlenik uspješno ažuriran');
        this.loadEmployees();
        this.closeModal();
      },
      error: (error) => {
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else if (error.error?.errors) {
          error.error.errors.forEach((err: string) => this.toastr.error(err));
        } else {
          this.toastr.error('Greška pri ažuriranju uposlenika');
        }
      },
    });
  }
}
