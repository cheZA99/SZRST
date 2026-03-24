import {
  HTTP_INTERCEPTORS,
  provideHttpClient,
  withInterceptorsFromDi,
} from '@angular/common/http';
import { APP_INITIALIZER, NgModule } from '@angular/core';
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
import { InitServiceService } from './services/init-service.service';
import { AuthInterceptor } from './interceptors/auth.interceptor';
import { FullCalendarModule } from '@fullcalendar/angular';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule } from '@angular/material/dialog';
import { AppointmentDialogComponent } from './components/rezervacije-dialog/rezervacije-dialog.component';
import { OverlayModule } from '@angular/cdk/overlay';
import { A11yModule } from '@angular/cdk/a11y';
import { OrganizacijeComponent } from './components/organizacije/organizacije.component';

import { TranslateModule, TranslateLoader } from '@ngx-translate/core';
import { HttpClientModule, HttpClient } from '@angular/common/http';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { ForgotPasswordComponent } from './components/forgot-password/forgot-password.component';

export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

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
    AppointmentDialogComponent,
    OrganizacijeComponent,
    ForgotPasswordComponent,
  ],
  bootstrap: [AppComponent],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule,
    ReactiveFormsModule,
    BrowserAnimationsModule,
    ToastrModule.forRoot(),
    FullCalendarModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatCheckboxModule,
    MatButtonModule,
    A11yModule,
    OverlayModule,
    HttpClientModule,
  TranslateModule.forRoot({
    defaultLanguage: 'bs',
    loader: {
      provide: TranslateLoader,
      useFactory: HttpLoaderFactory,
      deps: [HttpClient]
    }
  })
  ],
  providers: [
    provideHttpClient(withInterceptorsFromDi()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true,
    },
    {
      provide: APP_INITIALIZER,
      useFactory: initFactory,
      deps: [InitServiceService],
      multi: true,
    },
  ],
})
export class AppModule {}

export function initFactory(initService: InitServiceService) {
  return () => initService.init();
}
