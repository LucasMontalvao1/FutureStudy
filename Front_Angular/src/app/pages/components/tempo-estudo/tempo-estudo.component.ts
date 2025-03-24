import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-tempo-estudo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tempo-estudo.component.html',
  styleUrls: ['./tempo-estudo.component.scss']
})
export class TempoEstudoComponent {
  tempoTotal: string = '45h 30min';
  periodo: string = 'Esta semana';
}