import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TempoEstudoComponent } from '../components/tempo-estudo/tempo-estudo.component';
import { MetasComponent } from '../components/metas/metas.component';
import { MateriaPrincipalComponent } from '../components/materia-principal/materia-principal.component';
import { CalendarioComponent } from '../components/calendario/calendario.component';
import { CronometroComponent } from '../components/cronometro/cronometro.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    TempoEstudoComponent,
    MetasComponent,
    MateriaPrincipalComponent,
    CalendarioComponent,
    CronometroComponent
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss']
})
export class HomeComponent {}