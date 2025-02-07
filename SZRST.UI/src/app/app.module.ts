import { HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';
import { ToastrModule } from 'ngx-toastr';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RezervacijeComponent } from './components/rezervacije/rezervacije.component';
import { LokacijeComponent } from './components/lokacije/lokacije.component';
import { ResursiComponent } from './components/resursi/resursi.component';
import { KategorijeComponent } from './components/kategorije/kategorije.component';
import { LayoutComponent } from './components/layout/layout.component';
import { UposleniciComponent } from './components/uposlenici/uposlenici.component';
import { VijestiComponent } from './components/vijesti/vijesti.component';
import { IzvjestajiComponent } from './components/izvjestaji/izvjestaji.component';
@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    SignupComponent,
    DashboardComponent,
    RezervacijeComponent,
    LokacijeComponent,
    ResursiComponent,
    KategorijeComponent,
    LayoutComponent,
    UposleniciComponent,
    VijestiComponent,
    IzvjestajiComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot(),
  ],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}
