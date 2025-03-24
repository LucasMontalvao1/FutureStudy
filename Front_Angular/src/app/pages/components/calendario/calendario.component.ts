import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, PLATFORM_ID, Inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { CalendarioService, DiaCalendario, DetalhesDia } from '../../../services/calendario.service';
import { MetasService } from '../../../services/metas.service';
import { AnotacoesService } from '../../../services/anotacoes.service';
import { takeUntil } from 'rxjs/operators';
import { Subject, forkJoin, of } from 'rxjs';

interface DiaMes {
  numero: number;
  data: Date;
  tempoEstudo: string | null;
  minutosEstudados: number;
  className: string;
  desabilitado: boolean;
}

@Component({
  selector: 'app-calendario',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './calendario.component.html',
  styleUrls: ['./calendario.component.scss']
})
export class CalendarioComponent implements OnInit, OnDestroy {
  @Input() exibirDetalhes: boolean = true;
  @Input() permitirNavegacao: boolean = true;

  @Output() diaSelecionadoChange = new EventEmitter<Date>();
  @Output() detalhesDiaChange = new EventEmitter<DetalhesDia>();

  mesAtual: Date = new Date();
  nomeMes: string = '';
  diasSemana: string[] = ['Dom', 'Seg', 'Ter', 'Qua', 'Qui', 'Sex', 'Sáb'];
  dias: DiaMes[] = [];
  diasCalendario: DiaCalendario[] = [];
  diaSelecionado: Date | null = null;
  detalheDia: DetalhesDia | null = null;
  carregando: boolean = false;
  erro: string | null = null;
  isBrowser: boolean;

  private destroy$ = new Subject<void>();

  constructor(
    public calendarioService: CalendarioService,
    private metasService: MetasService,
    private anotacoesService: AnotacoesService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) { 
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  ngOnInit(): void {
    // Inicializar o calendário apenas no navegador
    if (this.isBrowser) {
      this.atualizarCalendario();
    } else {
      // No servidor, apenas inicializa as estruturas básicas
      this.atualizarNomeMes();
      this.carregarDiasDoMes();
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  atualizarCalendario(): void {
    this.atualizarNomeMes();
    this.carregarDiasDoMes();
    
    // Carregar dados da API apenas no navegador
    if (this.isBrowser) {
      this.carregarDadosCalendario();
    }
  }

  atualizarNomeMes(): void {
    const meses = [
      'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
      'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro'
    ];
    this.nomeMes = `${meses[this.mesAtual.getMonth()]} ${this.mesAtual.getFullYear()}`;
  }

  carregarDiasDoMes(): void {
    const ano = this.mesAtual.getFullYear();
    const mes = this.mesAtual.getMonth();
    const hoje = new Date();
    
    // Primeiro dia do mês
    const primeiroDia = new Date(ano, mes, 1);
    
    // Último dia do mês
    const ultimoDia = new Date(ano, mes + 1, 0);
    
    // Dia da semana em que o mês começa (0 = Domingo, 1 = Segunda, etc.)
    const diaSemanaInicio = primeiroDia.getDay();
    
    // Total de dias no mês
    const totalDias = ultimoDia.getDate();
    
    // Inicializar o array de dias
    this.dias = [];
    
    // Adicionar dias vazios até o primeiro dia do mês
    for (let i = 0; i < diaSemanaInicio; i++) {
      this.dias.push({
        numero: 0,
        data: new Date(ano, mes, 0),
        tempoEstudo: null,
        minutosEstudados: 0,
        className: 'vazio',
        desabilitado: true
      });
    }
    
    // Adicionar os dias do mês
    for (let i = 1; i <= totalDias; i++) {
      const dataDia = new Date(ano, mes, i);
      const ehHoje = dataDia.toDateString() === hoje.toDateString();
      const ehFuturo = dataDia > hoje;
      
      this.dias.push({
        numero: i,
        data: dataDia,
        tempoEstudo: null,
        minutosEstudados: 0,
        className: ehHoje ? 'hoje' : (ehFuturo ? 'futuro' : ''),
        desabilitado: ehFuturo
      });
    }
    
    // Completar com dias vazios até o final da semana
    const diasRestantes = 42 - this.dias.length; // 6 semanas completas
    for (let i = 0; i < diasRestantes; i++) {
      this.dias.push({
        numero: 0,
        data: new Date(ano, mes + 1, i + 1),
        tempoEstudo: null,
        minutosEstudados: 0,
        className: 'vazio',
        desabilitado: true
      });
    }
  }

  carregarDadosCalendario(): void {
    // Verificar se estamos no navegador
    if (!this.isBrowser) return;
    
    this.carregando = true;
    this.erro = null;

    this.calendarioService.getCalendario(
      this.mesAtual.getMonth() + 1, // API espera mês de 1 a 12
      this.mesAtual.getFullYear()
    ).subscribe({
      next: (data: DiaCalendario[]) => {
        this.diasCalendario = data;
        this.atualizarDiasComDados();
        this.carregando = false;
      },
      error: (err: any) => {
        console.error('Erro ao carregar dados do calendário', err);
        this.erro = 'Não foi possível carregar os dados do calendário.';
        this.carregando = false;
      }
    });
  }

  atualizarDiasComDados(): void {
    // Mapear os dados de dias estudados para o calendário
    this.diasCalendario.forEach(diaCalendario => {
      const diaMes = this.dias.find(d => d.numero === diaCalendario.dia);
      if (diaMes) {
        diaMes.minutosEstudados = diaCalendario.minutosEstudados;
        diaMes.tempoEstudo = this.formatarTempoEstudo(diaCalendario.minutosEstudados);
        
        // Adicionar classes baseadas no tempo estudado
        if (diaCalendario.minutosEstudados > 90) { // Mais de 1h30min
          diaMes.className += ' dia-estudo completo';
        } else if (diaCalendario.minutosEstudados > 0) {
          diaMes.className += ' dia-estudo parcial';
        }
      }
    });
  }

  // Método para formatar o tempo (usado no template)
  formatarTempoEstudo(minutos: number): string {
    if (!minutos || isNaN(minutos) || minutos <= 0) return '0h';
    
    const horas = Math.floor(minutos / 60);
    const min = minutos % 60;
    
    if (horas > 0 && min > 0) {
      return `${horas}h ${min}min`;
    } else if (horas > 0) {
      return `${horas}h`;
    } else {
      return `${min}min`;
    }
  }

  selecionarDia(dia: DiaMes): void {
    if (!this.isBrowser || dia.desabilitado || dia.numero === 0) return;
    
    // Remover seleção do dia anterior
    this.dias.forEach(d => {
      d.className = d.className.replace(' selecionado', '');
    });
    
    // Selecionar o novo dia
    dia.className += ' selecionado';
    this.diaSelecionado = dia.data;
    
    // Emitir evento
    this.diaSelecionadoChange.emit(dia.data);
    
    // Buscar detalhes do dia
    this.buscarDetalhesDia(dia.data);
  }

  buscarDetalhesDia(data: Date): void {
    if (!this.isBrowser || !this.exibirDetalhes) return;
    
    this.carregando = true;
    this.detalheDia = null;
    
    // Formatar data para YYYY-MM-DD para a API
    const dataFormatada = this.formatarData(data);

    // Formatar início e fim do dia para sessões
  const inicio = new Date(data);
  inicio.setHours(0, 0, 0, 0);
  
  const fim = new Date(data);
  fim.setHours(23, 59, 59, 999);
    
    // Buscar sessões de estudo
    const sessoes$ = this.calendarioService.getDetalhesDia(data);
    // Buscar metas para o dia
    const metas$ = this.metasService.getMetasByData(dataFormatada);
    // Buscar anotações para o dia
    const anotacoes$ = this.anotacoesService.getAnotacoesByData(dataFormatada);
    
    // Combinar todos os resultados em uma única chamada
    forkJoin({
      sessoes: sessoes$,
      metas: metas$,
      anotacoes: anotacoes$
    }).subscribe({
      next: (resultado) => {
        // Processar os detalhes com o método do serviço
        this.detalheDia = this.calendarioService.processarDetalhes(
          resultado.sessoes,
          resultado.metas,
          resultado.anotacoes
        );
        
        this.detalhesDiaChange.emit(this.detalheDia);
        this.carregando = false;
      },
      error: (err: any) => {
        console.error('Erro ao buscar detalhes do dia', err);
        this.erro = 'Não foi possível carregar os detalhes do dia.';
        this.carregando = false;
      }
    });
  }

  // Método auxiliar para formatar data como MM/dd/yyyy
  private formatarData(data: Date): string {
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    const ano = data.getFullYear();
    return `${mes}/${dia}/${ano}`;
  }

  // Método para formatar duração sem risco de NaN
  formatarDuracao(minutos: number): string {
    if (!minutos || isNaN(minutos)) {
      return '0min'; 
    }
    
    const horas = Math.floor(minutos / 60);
    const min = minutos % 60;
    
    if (horas > 0 && min > 0) {
      return `${horas}h ${min}min`;
    } else if (horas > 0) {
      return `${horas}h`;
    } else {
      return `${min}min`;
    }
  }

  // Formatar o status para exibição
  formatarStatus(status: string): string {
    switch (status && status.toLowerCase()) {
      case 'em_andamento':
        return 'Em andamento';
      case 'pausada':
        return 'Pausada';
      case 'concluida':
        return 'Finalizada';
      default:
        return 'Desconhecido';
    }
  }

  voltarMes(): void {
    if (!this.isBrowser || !this.permitirNavegacao) return;
    
    const novaData = new Date(this.mesAtual);
    novaData.setMonth(novaData.getMonth() - 1);
    this.mesAtual = novaData;
    this.atualizarCalendario();
  }

  avancarMes(): void {
    if (!this.isBrowser || !this.permitirNavegacao) return;
    
    const hoje = new Date();
    const ultimoDiaMesAtual = new Date(this.mesAtual.getFullYear(), this.mesAtual.getMonth() + 1, 0);
    
    // Verificar se o próximo mês está no futuro
    if (ultimoDiaMesAtual >= hoje) {
      return; // Não permitir avançar para meses futuros
    }
    
    const novaData = new Date(this.mesAtual);
    novaData.setMonth(novaData.getMonth() + 1);
    this.mesAtual = novaData;
    this.atualizarCalendario();
  }

  irParaHoje(): void {
    if (!this.isBrowser || !this.permitirNavegacao) return;
    
    const hoje = new Date();
    this.mesAtual = new Date(hoje.getFullYear(), hoje.getMonth(), 1);
    this.atualizarCalendario();
    
    // Esperar a atualização do calendário antes de selecionar o dia
    setTimeout(() => {
      const diaHoje = this.dias.find(d => 
        d.data.getDate() === hoje.getDate() && 
        d.data.getMonth() === hoje.getMonth() && 
        d.data.getFullYear() === hoje.getFullYear()
      );
      
      if (diaHoje) {
        this.selecionarDia(diaHoje);
      }
    }, 0);
  }

  formatarDataDetalhes(): string {
    if (!this.diaSelecionado) return '';
    
    const opcoes: Intl.DateTimeFormatOptions = { 
      weekday: 'long', 
      day: 'numeric', 
      month: 'long', 
      year: 'numeric' 
    };
    
    return this.diaSelecionado.toLocaleDateString('pt-BR', opcoes);
  }
}