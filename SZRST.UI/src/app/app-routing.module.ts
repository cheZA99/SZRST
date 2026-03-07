import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { RezervacijeComponent } from './components/rezervacije/rezervacije.component';
import { LayoutComponent } from './components/layout/layout.component';
import { LokacijeComponent } from './components/lokacije/lokacije.component';
import { ResursiComponent } from './components/resursi/resursi.component';
import { KategorijeComponent } from './components/kategorije/kategorije.component';
import { UposleniciComponent } from './components/uposlenici/uposlenici.component';
import { VijestiComponent } from './components/vijesti/vijesti.component';
import { IzvjestajiComponent } from './components/izvjestaji/izvjestaji.component';
import { authGuard } from './guards/auth.guard';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { roleGuard } from './guards/role.guard';
import { OrganizacijeComponent } from './components/organizacije/organizacije.component';
import { EditProfileComponent } from './components/edit-profile/edit-profile.component';

const routes: Routes = [
  { path: '', redirectTo: '/dashboard', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },

  {
    path: '',
    component: LayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent },

      {
        path: 'rezervacije',
        component: RezervacijeComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin', 'Admin', 'Uposlenik', 'Korisnik'] },
      },
      {
        path: 'organizacije',
        component: OrganizacijeComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin'] },
      },
      {
        path: 'lokacije',
        component: LokacijeComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin', 'Admin'] },
      },
      {
        path: 'resursi',
        component: ResursiComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin', 'Admin'] },
      },
      {
        path: 'kategorije',
        component: KategorijeComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin'] },
      },
      {
        path: 'uposlenici',
        component: UposleniciComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin', 'Admin'] },
      },
      {
        path: 'uredi-profil',
        component: EditProfileComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin', 'Admin', 'Uposlenik', 'Korisnik'] },
      },
      {
        path: 'vijesti',
        component: VijestiComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin'] },
      },
      {
        path: 'izvjestaji',
        component: IzvjestajiComponent,
        canActivate: [roleGuard],
        data: { roles: ['SuperAdmin', 'Admin'] },
      },
    ],
  },

  { path: '**', component: NotFoundComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule { }
