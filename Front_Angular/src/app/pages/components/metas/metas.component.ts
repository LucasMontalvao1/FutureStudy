import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-metas',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './metas.component.html',
  styleUrls: ['./metas.component.scss']
})
export class MetasComponent {
  metasAlcancadas: string = '5/7';
  periodo: string = 'Dias da semana';
}