import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TempoEstudoComponent } from '../components/tempo-estudo/tempo-estudo.component';
import { DiasEstudosComponent } from '../components/DiasEstudos/DiasEstudos.component';
import { MateriaPrincipalComponent } from '../components/materia-principal/materia-principal.component';
import { CalendarioComponent } from '../components/calendario/calendario.component';
import { CronometroComponent } from '../components/cronometro/cronometro.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    TempoEstudoComponent,
    DiasEstudosComponent,
    MateriaPrincipalComponent,
    CalendarioComponent,
    CronometroComponent
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {}