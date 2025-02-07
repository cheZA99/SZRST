import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import {RezervacijeComponent} from "./components/rezervacije/rezervacije.component";
import {LayoutComponent} from "./components/layout/layout.component";
import {LokacijeComponent} from "./components/lokacije/lokacije.component";
import {ResursiComponent} from "./components/resursi/resursi.component";
import {KategorijeComponent} from "./components/kategorije/kategorije.component";
import {UposleniciComponent} from "./components/uposlenici/uposlenici.component";
import {VijestiComponent} from "./components/vijesti/vijesti.component";
import {IzvjestajiComponent} from "./components/izvjestaji/izvjestaji.component";

const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  { path: '', component: LayoutComponent,
    children: [
      { path: 'dashboard', component: DashboardComponent }
      ,{ path: 'rezervacije', component: RezervacijeComponent },
      {path: 'lokacije', component: LokacijeComponent },
      {path: 'resursi', component: ResursiComponent },
      {path: 'kategorije', component: KategorijeComponent },
      {path: 'uposlenici', component: UposleniciComponent },
      {path: 'vijesti', component: VijestiComponent },
      {path: 'izvjestaji', component: IzvjestajiComponent },]},
];



@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
