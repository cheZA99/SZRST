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
  showDeleteModal = false;
  isEditMode = false;
  selectedEmployee: Employee | null = null;
  employeeForm: FormGroup;
  showPassword = false;
  showConfirmPassword = false;
  showNewPassword = false;
  showNewConfirmPassword = false;

  // Permisije
  isSuperAdmin = false;
  isAdmin = false;

  // Current user tenant ID - OVO JE KLJUČNO!
  currentUserTenantId: number | null = null;

  constructor(
    private uposleniciService: UposleniciService,
    private tenantService: TenantService,
    public authService: AuthService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {
    this.employeeForm = this.createForm();
  }

  private createForm(): FormGroup {
    // OVO JE VAŽNO: Dohvati tenantId na osnovu trenutnog korisnika
    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;


    if (this.authService.hasRole('SuperAdmin')) {
      // SuperAdmin mora odabrati organizaciju (obavezno)
      return this.fb.group(
        {
          userName: ['', [Validators.required, Validators.minLength(3)]],
          email: ['', [Validators.required, Validators.email]],
          password: ['', [Validators.required, Validators.minLength(6)]],
          confirmPassword: ['', [Validators.required]],
          tenantId: [null, Validators.required], // OBAVEZNO za SuperAdmina
          active: [true],
          newPassword: [''],
          newConfirmPassword: [''],
        },
        {
          validators: this.passwordMatchValidator,
        }
      );
    } else {
      // Admin automatski koristi svoju organizaciju - NEMA tenantId polja u formi
      return this.fb.group(
        {
          userName: ['', [Validators.required, Validators.minLength(3)]],
          email: ['', [Validators.required, Validators.email]],
          password: ['', [Validators.required, Validators.minLength(6)]],
          confirmPassword: ['', [Validators.required]],
          // NEMA tenantId polja za Admina!
          active: [true],
          newPassword: [''],
          newConfirmPassword: [''],
        },
        {
          validators: this.passwordMatchValidator,
        }
      );
    }
  }

  get authServiceInstance(): AuthService {
    return this.authService;
  }

  passwordMatchValidator(g: FormGroup) {
    const password = g.get('password')?.value;
    const confirmPassword = g.get('confirmPassword')?.value;
    const newPassword = g.get('newPassword')?.value;
    const newConfirmPassword = g.get('newConfirmPassword')?.value;

    const errors: any = {};

    if (password && password !== confirmPassword) {
      errors['passwordMismatch'] = true;
    }

    if (newPassword && newPassword !== newConfirmPassword) {
      errors['newPasswordMismatch'] = true;
    }

    return Object.keys(errors).length > 0 ? errors : null;
  }

  ngOnInit(): void {
    this.checkPermissions();
    this.loadEmployees();

    // OVO JE VAŽNO: Inicijalizuj current user tenant ID PRIJE svega
    const currentUser = this.authService.currentUser();
    this.currentUserTenantId = currentUser?.tenantId || null;


    // Ako je superadmin, učitaj organizacije
    if (this.isSuperAdmin) {
      this.loadTenants();
    }
  }

  checkPermissions(): void {
    this.isSuperAdmin = this.authService.hasRole('SuperAdmin');
    this.isAdmin = this.authService.hasRole('Admin');

  }

  loadEmployees(): void {
    this.loading = true;
    this.uposleniciService.getEmployees().subscribe({
      next: (data) => {
        this.employees = data;
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
      error: (error) => {
        console.error('Greška pri učitavanju organizacija:', error);
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loadingTenants = false;
      },
    });
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.selectedEmployee = null;

    if (this.isSuperAdmin) {
      // SuperAdmin mora odabrati organizaciju
      this.employeeForm.reset({
        active: true,
        tenantId: null, // Prazno, mora se odabrati
      });
    } else if (this.isAdmin) {
      // Admin automatski koristi svoju organizaciju
      this.employeeForm.reset({
        active: true,
        // NEMA tenantId polja!
      });
    }

    this.showModal = true;
  }

  openEditModal(employee: Employee): void {
    this.isEditMode = true;
    this.selectedEmployee = employee;


    if (this.isSuperAdmin) {
      this.employeeForm = this.fb.group(
        {
          userName: [
            employee.userName,
            [Validators.required, Validators.minLength(3)],
          ],
          email: [employee.email, [Validators.required, Validators.email]],
          active: [employee.active],
          tenantId: [employee.tenantId, Validators.required], // OBAVEZNO za SuperAdmina
          newPassword: [''],
          newConfirmPassword: [''],
        },
        {
          validators: this.passwordMatchValidator,
        }
      );
    } else {
      // Admin forma - NEMA tenantId polja
      this.employeeForm = this.fb.group(
        {
          userName: [
            employee.userName,
            [Validators.required, Validators.minLength(3)],
          ],
          email: [employee.email, [Validators.required, Validators.email]],
          active: [employee.active],
          // NEMA tenantId polja za Admina!
          newPassword: [''],
          newConfirmPassword: [''],
        },
        {
          validators: this.passwordMatchValidator,
        }
      );
    }

    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showPassword = false;
    this.showConfirmPassword = false;
    this.showNewPassword = false;
    this.showNewConfirmPassword = false;

    // Resetuj formu
    this.employeeForm = this.createForm();
    this.selectedEmployee = null;
  }

  togglePasswordVisibility(
    field: 'password' | 'confirm' | 'newPassword' | 'newConfirm'
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
    Object.keys(this.employeeForm.controls).forEach((key) => {
      this.employeeForm.get(key)?.markAsTouched();
    });

    if (this.employeeForm.errors?.['passwordMismatch']) {
      this.toastr.error('Lozinke se ne podudaraju');
    }
    if (this.employeeForm.errors?.['newPasswordMismatch']) {
      this.toastr.error('Nove lozinke se ne podudaraju');
    }

    return;
  }

  const formData = this.employeeForm.value;

  // Ako je Admin, dodaj tenantId automatski
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
  // Provjeri da li je tenantId postavljen za SuperAdmina
  if (this.isSuperAdmin && !data.tenantId) {
    this.toastr.error('Morate odabrati organizaciju');
    return;
  }

  // Ako je admin, postavi tenantId na svoju organizaciju
  if (this.isAdmin && !this.isSuperAdmin) {
    data.tenantId = this.currentUserTenantId;
  }

  console.log('Creating employee with data:', data);

  this.uposleniciService.createEmployee(data).subscribe({
    next: (response) => {
      this.toastr.success('Uposlenik uspješno kreiran');
      this.loadEmployees();
      this.closeModal();
    },
    error: (error) => {
      console.error('Greška pri kreiranju uposlenika:', error);
      if (error.error?.message) {
        this.toastr.error(error.error.message);
      } else if (error.error?.errors) {
        error.error.errors.forEach((err: string) => {
          this.toastr.error(err);
        });
      } else {
        this.toastr.error('Greška pri kreiranju uposlenika');
      }
    },
  });
}

updateEmployee(id: number, data: any): void {
  // Ako je admin, ne dozvoli promjenu tenantId
  if (this.isAdmin && !this.isSuperAdmin) {
    data.tenantId = this.selectedEmployee?.tenantId;
  }
  
  // U edit modu, ukloni confirmPassword jer ga nema u formi
  // ili je prazan, a backend ga očekuje
  delete data.confirmPassword;
  
  // Ako nije unesena nova lozinka, ukloni i password polja
  if (!data.newPassword) {
    delete data.newPassword;
    delete data.newConfirmPassword;
  } else {
    // Ako je unesena nova lozinka, postavi confirmPassword = newPassword
    // jer backend očekuje da se podudaraju
    data.confirmPassword = data.newPassword;
  }

  console.log('Updating employee with data:', data);

  this.uposleniciService.updateEmployee(id, data).subscribe({
    next: () => {
      this.toastr.success('Uposlenik uspješno ažuriran');
      this.loadEmployees();
      this.closeModal();
    },
    error: (error) => {
      console.error('Greška pri ažuriranju uposlenika:', error);
      if (error.error?.message) {
        this.toastr.error(error.error.message);
      } else if (error.error?.errors) {
        error.error.errors.forEach((err: string) => {
          this.toastr.error(err);
        });
      } else {
        this.toastr.error('Greška pri ažuriranju uposlenika');
      }
    },
  });
}
}
