import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-materia-principal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './materia-principal.component.html',
  styleUrls: ['./materia-principal.component.scss']
})
export class MateriaPrincipalComponent {
  materiaMaisEstudada: string = 'JavaScript';
  horasEstudadas: string = '15 horas esta semana';
}