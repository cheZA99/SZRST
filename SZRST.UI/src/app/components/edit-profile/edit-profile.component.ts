import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { UserService } from '../../services/user.service';
import { CityService, City } from '../../services/city.service';
import { CountryService, Country } from '../../services/country.service';
import { ToastrService } from 'ngx-toastr';
import { forkJoin } from 'rxjs';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-edit-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './edit-profile.component.html',
})
export class EditProfileComponent implements OnInit {
  private readonly maxImageSizeBytes = 2 * 1024 * 1024;
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

        this.imagePreview = this.resolveImageUrl(profile.imageUrl);

        if (profile.countryId) {
          this.onCountryChange(profile.countryId);
        }

        this.loading = false;
      },
      error: () => {
        this.toastr.error('Greska pri ucitavanju podataka');
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

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) {
      return;
    }

    if (!file.type.startsWith('image/')) {
      this.toastr.error('Molimo odaberite sliku');
      return;
    }

    if (file.size > this.maxImageSizeBytes) {
      this.toastr.error('Slika ne smije biti veca od 2MB');
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const base64 = reader.result as string;
      this.imagePreview = base64;

      this.userService.uploadProfileImage(base64).subscribe({
        next: (response) => {
          this.imagePreview = this.resolveImageUrl(response.imageUrl);
          this.toastr.success('Slika uspjesno postavljena');
        },
        error: () => {
          this.toastr.error('Greska pri postavljanju slike');
          this.imagePreview = null;
        },
      });
    };

    reader.readAsDataURL(file);
  }

  removeImage() {
    this.imagePreview = null;
    this.userService.uploadProfileImage('').subscribe({
      next: () => {
        this.toastr.success('Slika uklonjena');
      },
      error: () => {
        this.toastr.error('Greska pri uklanjanju slike');
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
      countryId: formValue.countryId || null,
      cityId: formValue.cityId || null,
    };

    this.userService.updateCurrentUserProfile(updateData).subscribe({
      next: () => {
        this.toastr.success('Profil uspjesno azuriran');
        this.profileForm.patchValue({
          currentPassword: '',
          newPassword: '',
          confirmPassword: '',
        });
        this.loading = false;
      },
      error: (error) => {
        this.toastr.error(
          error.error?.message || 'Greska pri azuriranju profila'
        );
        this.loading = false;
      },
    });
  }

  cancel() {
    this.router.navigate(['/dashboard']);
  }

  private resolveImageUrl(imageUrl?: string | null): string | null {
    if (!imageUrl) {
      return null;
    }

    if (
      imageUrl.startsWith('http://') ||
      imageUrl.startsWith('https://') ||
      imageUrl.startsWith('data:')
    ) {
      return imageUrl;
    }

    return `${environment.apiUrl}${imageUrl}`;
  }
}
