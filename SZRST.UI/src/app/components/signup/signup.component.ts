import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/ValidateForm';
import { AuthService } from 'src/app/services/auth.service';
import { TenantService, Tenant } from 'src/app/services/tenant.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-signup',
  templateUrl: './signup.component.html',
  styleUrls: ['./signup.component.css'],
})
export class SignupComponent implements OnInit {
  type: string = 'password';
  isText: boolean = false;
  eyeIcon: string = 'fa-eye-slash';
  signUpForm!: FormGroup;
  tenants: Tenant[] = [];
  isLoadingTenants: boolean = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private tenantService: TenantService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.signUpForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      username: ['', Validators.required],
      password: ['', Validators.required],
      confirmPassword: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      tenantId: ['', Validators.required], // Dodato
    });

    this.loadTenants();
  }

  loadTenants(): void {
    this.isLoadingTenants = true;
    this.tenantService.getAllTenants().subscribe({
      next: (tenants) => {
        this.tenants = tenants;
        this.isLoadingTenants = false;
      },
      error: (err) => {
        this.toastr.error('Greška pri učitavanju tenant-a');
        this.isLoadingTenants = false;
        console.error(err);
      },
    });
  }

  onSignUp() {
    console.log('Ovdje je');
    if (this.signUpForm.valid) {
      // Email validacija
      if (!/^\S+@\S+\.\S+$/.test(this.signUpForm.value.email)) {
        this.toastr.error(
          "Molimo unesite ispravnu e-poštu. Trebala bi slijediti format 'ime@primjer.com'."
        );
        return;
      }

      // Validacija da li je tenant odabran
      if (!this.signUpForm.value.tenantId) {
        this.toastr.error('Molimo odaberite organizaciju.');
        return;
      }

      this.authService.signUp(this.signUpForm.value).subscribe({
        next: (res) => {
          this.toastr.success(res.message);
          this.signUpForm.reset();
          this.router.navigate(['login']);
        },
        error: (err) => {
          this.toastr.error(err?.error.message);
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.signUpForm);
    }
  }

  hideShowPass() {
    this.isText = !this.isText;
    this.eyeIcon = this.isText ? 'fa-eye' : 'fa-eye-slash';
    this.type = this.isText ? 'text' : 'password';
  }
}
