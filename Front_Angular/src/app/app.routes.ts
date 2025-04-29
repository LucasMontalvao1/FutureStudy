import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { CronometroComponent } from './pages/components/cronometro/cronometro.component';
import { CalendarioComponent } from './pages/components/calendario/calendario.component';
import { CategoriaComponent } from './pages/cadastros/categoria/categoria.component';
import { MateriaComponent } from './pages/cadastros/materia/materia.component';

export const routes: Routes = [
  { 
    path: 'login', 
    loadComponent: () => import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  { path: 'home', component: HomeComponent },
  { path: 'cronometro', component: CronometroComponent },
  { path: 'calendario', component: CalendarioComponent },
  { path: 'categoria', component: CategoriaComponent },
  { path: 'materia', component: MateriaComponent },
  { path: '', redirectTo: 'login', pathMatch: 'full' }
];