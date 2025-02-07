import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/ValidateForm';
import { AuthService } from 'src/app/services/auth.service';
import { ToastrService } from 'ngx-toastr';
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
})
export class LoginComponent implements OnInit {
  type: string = 'password';
  isText: boolean = false;
  eyeIcon: string = 'fa-eye-slash';

  loginForm!: FormGroup;
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      email: ['', Validators.required],
      password: ['', Validators.required],
    });
  }

  hideShowPass() {
    this.isText = !this.isText;
    this.eyeIcon = this.isText ? 'fa-eye' : 'fa-eye-slash';
    this.type = this.isText ? 'text' : 'password';
  }

  onLogin() {
    if (this.loginForm.valid) {
      // Check if email follows a valid format
      if (!/^\S+@\S+\.\S+$/.test(this.loginForm.value.email)) {
        this.toastr.error(
          "Molimo unesite ispravnu e-poštu. Trebala bi slijediti format 'ime@primjer.com'."
        );
        return;
      }
      // Check if the password length is less than 5 characters
      /*if (this.loginForm.value.password.length < 5) {
        this.toastr.error(
          'Vaša lozinka mora biti dugačka minimalno 5 karaktera.'
        );
        return;
      }*/
      this.authService.login(this.loginForm.value).subscribe({
        next: () => {
          console.log("login success");
          this.loginForm.reset();
          this.router.navigate(['dashboard']);
        },
        error: (err) => {
          this.toastr.error(err?.error.message);
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.loginForm);
    }
  }
}
