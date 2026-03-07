import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import {
  Organizacija,
  OrganizacijeService,
} from '../../services/organizacije.service';

@Component({
  selector: 'app-organizacije',
  templateUrl: './organizacije.component.html',
  styleUrls: ['./organizacije.component.css'],
})
export class OrganizacijeComponent implements OnInit {
  organizacije: Organizacija[] = [];
  loading = false;
  showModal = false;
  showDeleteModal = false;
  isEditMode = false;
  selectedOrganizacija: Organizacija | null = null;
  organizacijaForm: FormGroup;
  showPassword = false;
  showConfirmPassword = false;

  constructor(
    private organizacijeService: OrganizacijeService,
    private fb: FormBuilder,
    private toastr: ToastrService
  ) {
    this.organizacijaForm = this.fb.group({
      tenantName: ['', [Validators.required, Validators.minLength(2)]],
      adminEmail: ['', [Validators.required, Validators.email]],
      adminUsername: ['', [Validators.required, Validators.minLength(3)]],
      adminPassword: ['', [Validators.required, Validators.minLength(6)]],
      adminConfirmPassword: ['', [Validators.required]],
    }, {
      validators: this.passwordMatchValidator
    });
  }

  passwordMatchValidator(g: FormGroup) {
    const password = g.get('adminPassword')?.value;
    const confirmPassword = g.get('adminConfirmPassword')?.value;
    return password === confirmPassword ? null : { mismatch: true };
  }

  ngOnInit(): void {
    this.loadOrganizacije();
  }

  loadOrganizacije(): void {
    this.loading = true;
    this.organizacijeService.getAll().subscribe({
      next: (data) => {
        this.organizacije = data;
        this.loading = false;
      },
      error: (error) => {
        this.toastr.error('Greška pri učitavanju organizacija');
        this.loading = false;
      },
    });
  }

  openCreateModal(): void {
    this.isEditMode = false;
    this.selectedOrganizacija = null;
    this.organizacijaForm.reset();
    this.showModal = true;
  }

  openEditModal(organizacija: Organizacija): void {
    this.isEditMode = true;
    this.selectedOrganizacija = organizacija;
    
    this.organizacijaForm = this.fb.group({
      name: [organizacija.name, [Validators.required, Validators.minLength(2)]],
    });
    
    this.showModal = true;
  }

  closeModal(): void {
    this.showModal = false;
    this.showPassword = false;
    this.showConfirmPassword = false;
    
    this.organizacijaForm = this.fb.group({
      tenantName: ['', [Validators.required, Validators.minLength(2)]],
      adminEmail: ['', [Validators.required, Validators.email]],
      adminUsername: ['', [Validators.required, Validators.minLength(3)]],
      adminPassword: ['', [Validators.required, Validators.minLength(6)]],
      adminConfirmPassword: ['', [Validators.required]],
    }, {
      validators: this.passwordMatchValidator
    });
    
    this.selectedOrganizacija = null;
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  toggleConfirmPasswordVisibility(): void {
    this.showConfirmPassword = !this.showConfirmPassword;
  }

  onSubmit(): void {
    if (this.organizacijaForm.invalid) {
      Object.keys(this.organizacijaForm.controls).forEach(key => {
        this.organizacijaForm.get(key)?.markAsTouched();
      });
      return;
    }

    const formData = this.organizacijaForm.value;

    if (this.isEditMode && this.selectedOrganizacija) {
      this.updateOrganizacija(this.selectedOrganizacija.id, { name: formData.name });
    } else {
      this.createOrganizacija(formData);
    }
  }

  createOrganizacija(data: any): void {
    this.organizacijeService.createWithAdmin(data).subscribe({
      next: (response) => {
        if (response.isSuccess) {
          this.toastr.success(response.message);
          this.loadOrganizacije();
          this.closeModal();
        } else {
          this.toastr.error(response.message);
        }
      },
      error: (error) => {
        if (error.error?.message) {
          this.toastr.error(error.error.message);
        } else {
          this.toastr.error('Greška pri kreiranju organizacije');
        }
      },
    });
  }

  updateOrganizacija(id: number, data: any): void {
    this.organizacijeService.update(id, data).subscribe({
      next: () => {
        this.toastr.success('Organizacija uspješno ažurirana');
        this.loadOrganizacije();
        this.closeModal();
      },
      error: (error) => {
        this.toastr.error('Greška pri ažuriranju organizacije');
      },
    });
  }

  openDeleteModal(organizacija: Organizacija): void {
    this.selectedOrganizacija = organizacija;
    this.showDeleteModal = true;
  }

  closeDeleteModal(): void {
    this.showDeleteModal = false;
    this.selectedOrganizacija = null;
  }

  confirmDelete(): void {
    if (this.selectedOrganizacija) {
      this.organizacijeService.delete(this.selectedOrganizacija.id).subscribe({
        next: () => {
          this.toastr.success('Organizacija uspješno obrisana');
          this.loadOrganizacije();
          this.closeDeleteModal();
        },
        error: (error) => {
          if (error.error?.message) {
            this.toastr.error(error.error.message);
          } else {
            this.toastr.error('Greška pri brisanju organizacije');
          }
          this.closeDeleteModal();
        },
      });
    }
  }
}