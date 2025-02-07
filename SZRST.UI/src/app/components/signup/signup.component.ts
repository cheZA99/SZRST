import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import ValidateForm from 'src/app/helpers/ValidateForm';
import { AuthService } from 'src/app/services/auth.service';
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
  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
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
      email: ['', Validators.required],
    });
  }
  onSignUp() {
    console.log("Ovdje je")
    if (this.signUpForm.valid) {
      debugger;
      // Check if email follows a valid format
      if (!/^\S+@\S+\.\S+$/.test(this.signUpForm.value.email)) {
        this.toastr.error(
          "Molimo unesite ispravnu e-poštu. Trebala bi slijediti format 'ime@primjer.com'."
        );
        return;
      }
      // Check if the password length is less than 5 characters
      /*if (
        this.signUpForm.value.password.length < 5 ||
        this.signUpForm.value.confirmPassword.length < 5
      ) {
        this.toastr.error(
          'Vaša lozinka mora biti dugačka minimalno 5 karaktera.'
        );
        return;
      }*/
      // if (!/\W/.test(this.signUpForm.value.password)) {
      //   // Password doesn't contain a non-alphanumeric character
      //   this.toastr.error(
      //     'Lozinke moraju sadržavati barem jedan znak koji nije slovo ili broj.'
      //   );
      //   return;
      // }
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
