import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-cronometro',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './cronometro.component.html',
  styleUrls: ['./cronometro.component.scss']
})
export class CronometroComponent {
  tempo: string = '00:45:00';
  categoria: string = 'Programação';
  materia: string = 'JavaScript';
  notas: string = '';
  
  iniciar(): void {
    console.log('Cronômetro iniciado');
  }
  
  pausar(): void {
    console.log('Cronômetro pausado');
  }
  
  parar(): void {
    console.log('Cronômetro parado');
  }
}