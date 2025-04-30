import { Component, OnInit, Inject, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { SessoesEstudoService } from '../../../services/sessoes-estudo.service';
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
  isBrowser: boolean;

  constructor(
    private dashboardService: SessoesEstudoService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    if (this.isBrowser) {
      setTimeout(() => {
        this.carregarDados();
      }, 100);
    } else {
      this.tempoTotal = '--';
    }
  }
  
  carregarDados(): void {
    this.dashboardService.getDashboard('semana', new Date()).subscribe({
      next: (res) => {
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