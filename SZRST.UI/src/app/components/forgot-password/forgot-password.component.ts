import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { AuthService } from 'src/app/services/auth.service';

@Component({
  selector: 'app-forgot-password',
  templateUrl: './forgot-password.component.html',
  styleUrls: ['./forgot-password.component.css'],
})
export class ForgotPasswordComponent implements OnInit {
  forgotPasswordForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.forgotPasswordForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
    });
  }

  onSubmit() {
    if (!this.forgotPasswordForm.valid) {
      return;
    }

    const email = this.forgotPasswordForm.value.email;
    if (!/^\S+@\S+\.\S+$/.test(email)) {
      this.toastr.error(
        "Molimo unesite ispravnu e-postu. Trebala bi slijediti format 'ime@primjer.com'."
      );
      return;
    }

    this.authService.forgotPassword(email).subscribe({
      next: (res) => {
        if (res.isSuccess) {
          this.toastr.success(
            res.message || 'Link za reset lozinke je poslan na email adresu.'
          );
          this.router.navigate(['login']);
          return;
        }

        this.toastr.error(res.message || 'Slanje reset linka nije uspjelo.');
      },
      error: (err) => {
        this.toastr.error(
          err?.error?.message || 'Slanje reset linka nije uspjelo.'
        );
      },
    });
  }
}
