import {Component, OnInit} from '@angular/core';
import { HttpClient } from "@angular/common/http";

@Component({
  selector: 'app-lokacije',
  templateUrl: './lokacije.component.html',
  styleUrls: ['./lokacije.component.css']
})
export class LokacijeComponent implements OnInit {
  public activeTab: string = 'overview';
  private baseUrl: string = 'https://localhost:5001/api/';
  public data: any[]=[];
  public facilities: any[]=[];
  public countries: any[]=[];
  public cities: any[]=[];
  public facilityTypes: any[]=[];
  public filteredCities: any[]=[];
  public selectedCountry: string='';
  public selectedCity: string='';
  public address: string='';
  public addresNumber: string = '';
  public facilityName: string = '';
  public facilityType: string = '';
  public searchType: string = '';
  public searchText: string = '';
  constructor(private http: HttpClient) {}

  changeTab(tab: string) {
    this.activeTab = tab;
  }

  getLocations(): void {
    this.http.get<any>(this.baseUrl+'Location/').subscribe(res => {
      this.data = res;
      console.log(this.data);
    });
  }

  getFacilities(filer:string,value:string): void {
    this.http.get<any>(this.baseUrl+`Facility/?filter=${filer}&value=${value}`).subscribe(res => {
      this.facilities = res;
      console.log(this.facilities);
    });
  }

  getCountries(): void {
    this.http.get<any>(this.baseUrl+'Country/').subscribe(res => {
      this.countries = res;
      console.log(this.countries);
    });
  }
  getCities(): void {
    this.http.get<any>(this.baseUrl+'City/').subscribe(res => {
      this.cities = res;
      console.log(this.cities);
    });
  }

  getFacilityTypes(): void {
    this.http.get<any>(this.baseUrl+'FacilityType/').subscribe(res => {
      this.facilityTypes = res;
      this.facilityType=this.facilityTypes[0].name;
      console.log(this.facilityTypes);
    });
  }
  ngOnInit(): void {
  this.getLocations();
  this.getCountries();
  this.getCities();
  this.getFacilityTypes();
  this.getFacilities("","");

  }

  getCountry() {
    this.filteredCities=this.cities.filter(x=> x.country.name==this.selectedCountry);
    this.selectedCity=this.filteredCities[0].name;
    console.log(this.selectedCountry);
    console.log( this.filteredCities);
  }

  addFacility() {
    console.log(this.selectedCity);
    let countryId = this.countries.find(x=> x.name==this.selectedCountry).id;
    let cityId = this.filteredCities.find(x=> x.name==this.selectedCity).id;
    let facilityTypeId = this.facilityTypes.find(x=> x.name==this.facilityType).id;
    let location = {
      "name": this.facilityName,
      "facilityTypeId":facilityTypeId,
      "address": this.address,
      "addressNumber": this.addresNumber,
      "countryId": countryId,
      "cityId": cityId
    }
    this.http.post<any>(this.baseUrl+'Facility/AddFacility/', location).subscribe(res => {
      this.getFacilities("","");
      this.activeTab = 'overview';
    })
  }
}
