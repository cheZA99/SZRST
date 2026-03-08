import { Component, OnInit } from '@angular/core';
import { dA } from '@fullcalendar/core/internal-common';
import { ToastrService } from 'ngx-toastr';
import { CityService } from 'src/app/services/city.service';
import { CountryService } from 'src/app/services/country.service';
import { CurrencyService } from 'src/app/services/currency.service';
import { FacilityTypeService } from 'src/app/services/facility-type.service';

@Component({
  selector: 'app-kategorije',
  templateUrl: './kategorije.component.html',
  styleUrls: ['./kategorije.component.css']
})
export class KategorijeComponent implements OnInit {

  cities: any[] = []
  countries: any[] = []
  facilityTypes: any[] = []
  currencies: any[] = []

  loading = false;

  cityForm: any = {
    name: '',
    countryId: null,
    isDeleted: false
  }

  countryForm: any = {
    name: '',
    shortName: '',
    currencyId: null
  }

  facilityTypeForm: any = {
    name: '',
    description: ''
  }

    currencyForm: any = {
    name: '',
    shortName: ''
  }

  constructor(private cityService: CityService
    , private countryService: CountryService,
    private facilityTypeService: FacilityTypeService,
    private currencyService: CurrencyService,
    private toastr: ToastrService) { }

  ngOnInit(): void {
    this.loadCities()
    this.loadCountries()
    this.loadFacilityTypes()
    this.loadCurrencies()
  }

  loadCities() {
    this.cityService.getCities().subscribe({
      next: (data) => {
        this.cities = data;
        console.log(this.cities)
      },
      error: (error) => {
        console.error('Greška pri učitavanju gradova:', error);
        this.toastr.error('Greška pri učitavanju gradova');
        this.loading = false;
      }
    })
  }

  loadCurrencies() {
    this.currencyService.getAll().subscribe({
      next: (data) => {
        this.currencies = data;
      },
      error: (error) => {
        console.error('Greška pri učitavanju valuta:', error);
        this.toastr.error('Greška pri učitavanju valuta');
        this.loading = false;
      }
    })
  }

  loadFacilityTypes() {
    this.facilityTypeService.getAll().subscribe({
      next: (data) => {
        this.facilityTypes = data;
      },
      error: (error) => {
        console.error('Greška pri učitavanju tipova objekata:', error);
        this.toastr.error('Greška pri učitavanju tipova objekata');
        this.loading = false;
      }
    })
  }

  loadCountries() {
    this.countryService.getCountries().subscribe({
      next: (data) => {
        this.countries = data;
        console.log(this.cities)
      },
      error: (error) => {
        console.error('Greška pri učitavanju država:', error);
        this.toastr.error('Greška pri učitavanju država');
        this.loading = false;
      }
    })
  }

  openCityModal() {
    (document.getElementById('cityModal') as any).showModal()
  }

  openCountryModal() {
    (document.getElementById('countryModal') as any).showModal()
  }

  openFacilityModal() {
    (document.getElementById('facilityModal') as any).showModal()
  }

  openCurrencyModal() {
    (document.getElementById('currencyModal') as any).showModal()
  }

  closeCityModal() {
    (document.getElementById('cityModal') as any).close()
  }

  closeCountryModal() {
    (document.getElementById('countryModal') as any).close()
  }

  closeFacilityTypeModal() {
    (document.getElementById('facilityModal') as any).close()
  }

  closeCurrencyModal() {
    (document.getElementById('currencyModal') as any).close()
  }

  saveCity() {
    this.cityService.create(this.cityForm).subscribe({
      next: () => {
        this.loadCities();
        this.closeCityModal();
      },
      error: (error: any) => {
        console.error('Greška pri kreiranju grada:', error);
        this.toastr.error('Greška pri kreiranju grada');
        this.loading = false;
      }
    })
    this.closeCityModal()
  }
  

  saveFacilityType() {
    this.facilityTypeService.create(this.facilityTypeForm).subscribe({
      next: () => {
        this.loadFacilityTypes();
        this.closeFacilityTypeModal();
      },
      error: (error: any) => {
        console.error('Greška pri kreiranju tipa objekta:', error);
        this.toastr.error('Greška pri kreiranju tipa objekta');
        this.loading = false;
      }
    })
    this.closeFacilityTypeModal()
  }

    saveCurrency() {
    this.currencyService.create(this.currencyForm).subscribe({
      next: () => {
        this.loadCurrencies();
        this.closeCurrencyModal();
      },
      error: (error: any) => {
        console.error('Greška pri kreiranju valute:', error);
        this.toastr.error('Greška pri kreiranju valute');
        this.loading = false;
      }
    })
    this.closeCurrencyModal()
  }

  saveCountry() {
    this.countryService.create(this.countryForm).subscribe({
      next: () => {
        this.loadCountries();
        this.closeCountryModal();
      },
      error: (error) => {
        console.error('Greška pri kreiranju države:', error);
        this.toastr.error('Greška pri kreiranju države');
        this.loading = false;
      }
    })
    this.closeCountryModal()
  }
}
