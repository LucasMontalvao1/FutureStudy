import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessoesEstudoService } from '../../../services/sessoes-estudo.service';
import { SessaoDashboardStatsDto } from '../../../models/sessao-estudo.models';

@Component({
  selector: 'app-materia-principal',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './materia-principal.component.html',
  styleUrls: ['./materia-principal.component.scss']
})
export class MateriaPrincipalComponent implements OnInit {
  materiaMaisEstudada: string = '...';
  horasEstudadas: string = '...';

  constructor(private dashboardService: SessoesEstudoService) {}

  ngOnInit(): void {
    this.dashboardService.getDashboard('semana').subscribe({
      next: (res: SessaoDashboardStatsDto | null) => {
        if (res) {
          this.materiaMaisEstudada = res.materiaMaisEstudada ?? '--';
          this.horasEstudadas = this.formatarHoras(res.horasMateriaMaisEstudada);
        } else {
          this.materiaMaisEstudada = '--';
          this.horasEstudadas = '--';
        }
      },
      error: (err) => {
        console.error('Erro ao carregar matéria mais estudada:', err);
        this.materiaMaisEstudada = '--';
        this.horasEstudadas = '--';
      }
    });
  }

  formatarHoras(horas: number): string {
    const h = Math.floor(horas);
    const m = Math.round((horas - h) * 60);
    return `${h}h ${m}min esta semana`;
  }
}
