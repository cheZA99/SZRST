import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
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
    bootstrap: [AppComponent], imports: [BrowserModule,
        AppRoutingModule,
        FormsModule,
        ReactiveFormsModule,
        BrowserAnimationsModule,
        ToastrModule.forRoot()],
    providers: [
        provideHttpClient(withInterceptorsFromDi()),
        {
            provide: APP_INITIALIZER,
            useFactory: initFactory,
            deps: [InitServiceService],
            multi: true,
        },
    ]
})
export class AppModule { }

export function initFactory(initService: InitServiceService) {
  return () => initService.init();
  /*return new Promise<void>((resolve) => {
    setTimeout(async () => {
        try {
            initService.init();
        } finally {
            const splash = document.getElementById('initial-splash');
            if(splash){
                splash.remove();
            }
            resolve()
        }
    },500)
  })*/
}
