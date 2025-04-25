import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CronometroComponent } from './pages/components/cronometro/cronometro.component';
import { CalendarioComponent } from './pages/components/calendario/calendario.component';

export const routes: Routes = [
  { 
    path: 'login', 
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  { path: 'home', component: HomeComponent },
  { path: 'cronometro', component: CronometroComponent },
  { path: 'calendario', component: CalendarioComponent },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];