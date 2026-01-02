import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { UserService, UserProfile } from '../../services/user.service';
import { CityService, City } from '../../services/city.service';
import { CountryService, Country } from '../../services/country.service';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-profile.component.html',
})
export class EditProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private userService = inject(UserService);
  private cityService = inject(CityService);
  private countryService = inject(CountryService);
  private toastr = inject(ToastrService);
  private router = inject(Router);

  profileForm!: FormGroup;
  loading = false;
  imagePreview: string | null = null;
  showCurrentPassword = false;
  showNewPassword = false;
  showConfirmPassword = false;

  countries: Country[] = [];
  cities: City[] = [];
  filteredCities: City[] = [];

  ngOnInit() {
    this.initializeForm();
    this.loadData();
  }

  initializeForm() {
    this.profileForm = this.fb.group(
      {
        userName: ['', [Validators.required, Validators.minLength(3)]],
        email: ['', [Validators.required, Validators.email]],
        displayName: ['', [Validators.required, Validators.minLength(2)]],
        dateOfBirth: [''],
        gender: [''],
        description: [''],
        countryId: [''],
        cityId: [''],
        currentPassword: [''],
        newPassword: [''],
        confirmPassword: [''],
      },
      {
        validators: this.passwordMatchValidator,
      }
    );

    // Listen to country changes to filter cities
    this.profileForm.get('countryId')?.valueChanges.subscribe((countryId) => {
      this.onCountryChange(countryId);
    });
  }

  passwordMatchValidator(form: FormGroup) {
    const newPassword = form.get('newPassword')?.value;
    const confirmPassword = form.get('confirmPassword')?.value;

    if (newPassword && confirmPassword && newPassword !== confirmPassword) {
      return { mismatch: true };
    }
    return null;
  }

  loadData() {
    this.loading = true;

    forkJoin({
      profile: this.userService.getCurrentUserProfile(),
      countries: this.countryService.getCountries(),
      cities: this.cityService.getCities(),
    }).subscribe({
      next: ({ profile, countries, cities }) => {
        this.countries = countries;
        this.cities = cities;

        this.profileForm.patchValue({
          userName: profile.userName,
          email: profile.email,
          displayName: profile.displayName || profile.userName,
          dateOfBirth: profile.dateOfBirth,
          gender: profile.gender,
          description: profile.description,
          countryId: profile.countryId || '',
          cityId: profile.cityId || '',
        });

        if (profile.imageUrl) {
          this.imagePreview = profile.imageUrl;
        }

        // Filter cities based on selected country
        if (profile.countryId) {
          this.onCountryChange(profile.countryId);
        }

        this.loading = false;
      },
      error: (error) => {
        this.toastr.error('Greška pri učitavanju podataka');
        this.loading = false;
      },
    });
  }

  onCountryChange(countryId: number | string) {
    if (countryId) {
      this.filteredCities = this.cities.filter(
        (city) => city.country.id === +countryId
      );

      const currentCityId = this.profileForm.get('cityId')?.value;
      if (currentCityId) {
        const cityBelongsToCountry = this.filteredCities.some(
          (c) => c.id === +currentCityId
        );
        if (!cityBelongsToCountry) {
          this.profileForm.patchValue({ cityId: '' });
        }
      }
    } else {
      this.filteredCities = [];
      this.profileForm.patchValue({ cityId: '' });
    }
  }
  onFileSelected(event: any) {
    const file = event.target.files[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        this.toastr.error('Molimo odaberite sliku');
        return;
      }

      if (file.size > 5 * 1024 * 1024) {
        this.toastr.error('Slika ne smije biti veća od 5MB');
        return;
      }

      const reader = new FileReader();
      reader.onload = () => {
        const base64 = reader.result as string;
        this.imagePreview = base64;

        this.userService.uploadProfileImage(base64).subscribe({
          next: (response) => {
            this.toastr.success('Slika uspješno postavljena');
          },
          error: (error) => {
            this.toastr.error('Greška pri postavljanju slike');
            this.imagePreview = null;
          },
        });
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage() {
    this.imagePreview = null;
    this.userService.uploadProfileImage('').subscribe({
      next: () => {
        this.toastr.success('Slika uklonjena');
      },
      error: () => {
        this.toastr.error('Greška pri uklanjanju slike');
      },
    });
  }

  togglePasswordVisibility(field: 'current' | 'new' | 'confirm') {
    if (field === 'current') {
      this.showCurrentPassword = !this.showCurrentPassword;
    } else if (field === 'new') {
      this.showNewPassword = !this.showNewPassword;
    } else {
      this.showConfirmPassword = !this.showConfirmPassword;
    }
  }

  onSubmit() {
    if (this.profileForm.invalid) {
      this.toastr.error('Molimo popunite sva obavezna polja');
      return;
    }

    const formValue = this.profileForm.value;

    if (formValue.newPassword) {
      if (!formValue.currentPassword) {
        this.toastr.error('Unesite trenutnu lozinku');
        return;
      }
      if (formValue.newPassword.length < 6) {
        this.toastr.error('Nova lozinka mora imati najmanje 6 karaktera');
        return;
      }
    }

    this.loading = true;
    const updateData = {
      ...formValue,
      imageUrl: this.imagePreview,
      countryId: formValue.countryId || null,
      cityId: formValue.cityId || null,
    };

    this.userService.updateCurrentUserProfile(updateData).subscribe({
      next: () => {
        this.toastr.success('Profil uspješno ažuriran');
        this.profileForm.patchValue({
          currentPassword: '',
          newPassword: '',
          confirmPassword: '',
        });
        this.loading = false;
      },
      error: (error) => {
        this.toastr.error(
          error.error?.message || 'Greška pri ažuriranju profila'
        );
        this.loading = false;
      },
    });
  }

  cancel() {
    this.router.navigate(['/dashboard']);
  }
}
