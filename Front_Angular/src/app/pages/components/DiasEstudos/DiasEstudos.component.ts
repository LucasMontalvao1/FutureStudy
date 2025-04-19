import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SessoesEstudoService } from '../../../services/sessoes-estudo.service';
import { SessaoDashboardStatsDto } from '../../../models/sessao-estudo.models';

@Component({
  selector: 'app-metas',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './DiasEstudos.component.html',
  styleUrls: ['./DiasEstudos.component.scss']
})
export class DiasEstudosComponent implements OnInit {
  diasEstudados: number = 0;
  percentual: number = 0;
  periodo: string = 'Esta semana';

  constructor(private dashboardService: SessoesEstudoService) {}

  ngOnInit(): void {
    this.dashboardService.getDashboard('semana').subscribe({
      next: (res: SessaoDashboardStatsDto | null) => {
        if (res) {
          this.diasEstudados = res.diasEstudados || 0;
          this.percentual = res.percentualDiasEstudados || 0;
        }
      },
      error: (err) => {
        console.error('Erro ao carregar dados:', err);
      }
    });
  }
}