import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessoesEstudoService } from '../../../services/sessoes-estudo.service'; // Ajuste o caminho
import { SessaoDashboardStatsDto } from '../../../models/sessao-estudo.models';

@Component({
  selector: 'app-tempo-estudo',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './tempo-estudo.component.html',
  styleUrls: ['./tempo-estudo.component.scss']
})
export class TempoEstudoComponent implements OnInit {
  tempoTotal: string = '...';
  periodo: string = 'Esta semana';

  constructor(private dashboardService: SessoesEstudoService) {}

  ngOnInit(): void {
    this.dashboardService.getDashboard('semana').subscribe({
      next: (res: SessaoDashboardStatsDto | null) => {
        if (res && res.tempoTotalEstudado != null) {
          this.tempoTotal = this.formatarTempo(res.tempoTotalEstudado);
        } else {
          console.warn('Dashboard vazio ou tempoTotalEstudado ausente:', res);
          this.tempoTotal = '--';
        }
      },
      error: (err) => {
        console.error('Erro ao carregar dados do dashboard:', err);
        this.tempoTotal = '--';
      }
    });
  }
  

  formatarTempo(timeSpan: string): string {
    // Ex: "02:30:00" => 2h 30min
    const parts = timeSpan.split(':');
    const horas = parseInt(parts[0], 10);
    const minutos = parseInt(parts[1], 10);
    return `${horas}h ${minutos}min`;
  }
}
