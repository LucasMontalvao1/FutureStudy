import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, PLATFORM_ID, Inject } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { CalendarioService, DiaCalendario, DetalhesDia, MateriaResumo, TopicoResumo, CategoriaResumo } from '../../../services/calendario.service';
import { MetasService } from '../../../services/metas.service';
import { AnotacoesService } from '../../../services/anotacoes.service';
import { takeUntil } from 'rxjs/operators';
import { Subject, forkJoin, of } from 'rxjs';
import { ViewChild } from '@angular/core';

interface DiaMes {
  numero: number;
  data: Date;
  tempoEstudo: string | null;
  minutosEstudados: number;
  className: string;
  desabilitado: boolean;
}

// Interface para melhor tipagem dos dados agregados
interface SessaoAgrupada {
  id: number;
  materiaId: number;
  materiaNome: string;
  topicoId: number | null;
  topicoNome: string | null;
  categoriaId: string;
  categoriaNome: string;
  categoriaCor: string;
  dataInicio: Date;
  dataFim: Date | null;
  duracao: number;
  duracaoFormatada: string;
  status: string;
  statusNormalizado: string;
  anotacoes: any[]; 
  expandido: boolean; 
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
  @Input() agruparPorCategoria: boolean = true;

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

  // Propriedades para filtragem e agrupamento
  filtroCategoria: string | null = null;
  filtroMateria: number | null = null;
  filtroTopico: number | null = null;
  visualizacaoAtiva: 'tudo' | 'categoria' | 'materia' | 'topico' = 'tudo';
  
  // Novas propriedades para melhorias
  sessoesAgrupadas: SessaoAgrupada[] = [];
  anotacoesSemSessao: any[] = []; 
  categoriasDisponiveis: CategoriaResumo[] = [];
  materiasDisponiveis: MateriaResumo[] = [];
  topicosDisponiveis: TopicoResumo[] = [];
  
  // Estatísticas de estudo
  tempoTotalPorCategoria: {[key: string]: number} = {};
  mediaTempoEstudoDiasAnteriores: number = 0;
  tendenciaEstudo: 'subindo' | 'estavel' | 'descendo' = 'estavel';

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
        this.calcularMediaTempoEstudo(); // Nova função para calcular estatísticas
        this.carregando = false;
      },
      error: (err: any) => {
        console.error('Erro ao carregar dados do calendário', err);
        this.erro = 'Não foi possível carregar os dados do calendário.';
        this.carregando = false;
      }
    });
  }

  // Nova função para calcular estatísticas
  calcularMediaTempoEstudo(): void {
    // Filtra apenas os dias passados (não incluindo o dia atual)
    const hoje = new Date();
    const diasPassados = this.diasCalendario.filter(dia => {
      const data = new Date(this.mesAtual.getFullYear(), this.mesAtual.getMonth(), dia.dia);
      return data < hoje;
    });
    
    if (diasPassados.length > 0) {
      const totalMinutos = diasPassados.reduce((total, dia) => total + dia.minutosEstudados, 0);
      this.mediaTempoEstudoDiasAnteriores = totalMinutos / diasPassados.length;
    }
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
    return this.calendarioService.formatarTempoEstudo(minutos);
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
    this.resetarFiltros();
    this.depurarDados();
    
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
    
    // Adicionar essa linha para reconstruir os dados baseado nas sessões
    this.reconstruirDadosAgrupados();
    
    // Continuar com o restante do código...
    this.processarSessoesEAnotacoes();
    this.extrairFiltrosDisponiveis();
    this.calcularTempoTotalPorCategoria();
    this.determinarTendenciaEstudo();
    
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

  // Método para obter nome da matéria pelo ID
getMaterialNome(id: number): string {
  if (!this.detalheDia?.materias) return '';
  const materia = this.detalheDia.materias.find(m => m.id === id);
  return materia?.nome || '';
}

// Método para obter nome do tópico pelo ID
getTopicoNome(id: number): string {
  if (!this.detalheDia?.topicos) return '';
  const topico = this.detalheDia.topicos.find(t => t.id === id);
  return topico?.nome || '';
}

  // Nova função para processar sessões e associar anotações
  processarSessoesEAnotacoes(): void {
    if (!this.detalheDia) return;
    
    // Mapear as sessões para o novo formato com mais informações
    this.sessoesAgrupadas = this.detalheDia.sessoes.map(sessao => {
      // Encontrar a categoria da matéria
      const materia = this.detalheDia?.materias.find(m => m.id === sessao.materiaId);
      const categoria = this.detalheDia?.categorias.find(c => c.id === materia?.categoria);
      
      // Retornar a sessão com informações enriquecidas
      console.log(sessao);
      return {
        id: sessao.id,
        materiaId: sessao.materiaId,
        materiaNome: sessao.nomeMateria,
        topicoId: sessao.topicoId,
        topicoNome: sessao.nomeTopico,
        categoriaId: sessao.categoriaId,
        categoriaNome: sessao.nomeCategoria || 'Sem categoria',
        categoriaCor: categoria?.cor || '#808080',
        dataInicio: sessao.dataInicio,
        dataFim: sessao.dataFim,
        duracao: sessao.duracao,
        duracaoFormatada: this.formatarDuracao(sessao.duracao),
        status: sessao.status,
        statusNormalizado: sessao.statusNormalizado || this.normalizarStatus(sessao.status),
        anotacoes: [],
        expandido: true
      };
      
    });
    
    // Associar anotações às sessões
    this.anotacoesSemSessao = [...(this.detalheDia.anotacoes || [])];
    
    if (this.detalheDia.anotacoes) {
      // Para cada anotação, verificar se pertence a alguma sessão
      this.detalheDia.anotacoes.forEach(anotacao => {
        if (anotacao.sessaoId) {
          // Encontrar a sessão correspondente
          const sessao = this.sessoesAgrupadas.find(s => s.id === anotacao.sessaoId);
          if (sessao) {
            sessao.anotacoes.push(anotacao);
            // Remover da lista de anotações sem sessão
            const index = this.anotacoesSemSessao.findIndex(a => a.id === anotacao.id);
            if (index !== -1) {
              this.anotacoesSemSessao.splice(index, 1);
            }
          }
        }
      });
    }
  }

  // Nova função para extrair filtros disponíveis
  extrairFiltrosDisponiveis(): void {
    if (!this.detalheDia) return;
    
    this.categoriasDisponiveis = this.detalheDia.categorias;
    this.materiasDisponiveis = this.detalheDia.materias;
    this.topicosDisponiveis = this.detalheDia.topicos;
  }
  
  // Nova função para calcular distribuição de tempo por categoria
  calcularTempoTotalPorCategoria(): void {
    if (!this.detalheDia) return;
    
    this.tempoTotalPorCategoria = {};
    
    // Inicializar categorias
    this.detalheDia.categorias.forEach(categoria => {
      this.tempoTotalPorCategoria[categoria.id] = 0;
    });
    
    // Somar tempo de cada sessão à sua categoria
    this.sessoesAgrupadas.forEach(sessao => {
      if (this.tempoTotalPorCategoria[sessao.categoriaId] !== undefined) {
        this.tempoTotalPorCategoria[sessao.categoriaId] += sessao.duracao;
      }
    });
  }
  
  // Nova função para determinar tendência de estudo
  determinarTendenciaEstudo(): void {
    if (!this.detalheDia) return;
    
    const minutosHoje = this.detalheDia.totalMinutos || 0;
    
    if (minutosHoje > this.mediaTempoEstudoDiasAnteriores * 1.2) {
      this.tendenciaEstudo = 'subindo';
    } else if (minutosHoje < this.mediaTempoEstudoDiasAnteriores * 0.8) {
      this.tendenciaEstudo = 'descendo';
    } else {
      this.tendenciaEstudo = 'estavel';
    }
  }

  // Método auxiliar para normalizar status
  normalizarStatus(status: string | undefined): string {
    if (!status) return 'desconhecido';
    
    const statusLowerCase = status.toLowerCase();
    
    if (statusLowerCase.includes('andamento')) return 'emandamento';
    if (statusLowerCase.includes('pausa')) return 'pausada';
    if (statusLowerCase.includes('final') || statusLowerCase.includes('conclu')) return 'finalizada';
    
    return 'desconhecido';
  }

  // Método para resetar filtros
  resetarFiltros(): void {
    this.filtroCategoria = null;
    this.filtroMateria = null;
    this.filtroTopico = null;
    this.visualizacaoAtiva = 'tudo';
  }

  // Métodos para filtrar conteúdo
  aplicarFiltroCategoria(categoriaId: string): void {
    this.resetarFiltros();
    this.filtroCategoria = categoriaId;
    this.visualizacaoAtiva = 'categoria';
  }

  aplicarFiltroMateria(materiaId: number): void {
    this.resetarFiltros();
    this.filtroMateria = materiaId;
    this.visualizacaoAtiva = 'materia';
  }

  aplicarFiltroTopico(topicoId: number): void {
    this.resetarFiltros();
    this.filtroTopico = topicoId;
    this.visualizacaoAtiva = 'topico';
  }

  // Métodos para obter dados filtrados
  getSessoesFiltered(): SessaoAgrupada[] {
    if (this.visualizacaoAtiva === 'categoria' && this.filtroCategoria) {
      return this.sessoesAgrupadas.filter(s => s.categoriaId === this.filtroCategoria);
    }
    
    if (this.visualizacaoAtiva === 'materia' && this.filtroMateria) {
      return this.sessoesAgrupadas.filter(s => s.materiaId === this.filtroMateria);
    }
    
    if (this.visualizacaoAtiva === 'topico' && this.filtroTopico) {
      return this.sessoesAgrupadas.filter(s => s.topicoId === this.filtroTopico);
    }
    
    return this.sessoesAgrupadas;
  }

  getAnotacoesFiltered(): any[] {
    if (this.visualizacaoAtiva === 'categoria' && this.filtroCategoria) {
      return this.anotacoesSemSessao.filter(a => a.categoriaMateria === this.filtroCategoria);
    }
    
    if (this.visualizacaoAtiva === 'materia' && this.filtroMateria) {
      return this.anotacoesSemSessao.filter(a => a.materiaId === this.filtroMateria);
    }
    
    if (this.visualizacaoAtiva === 'topico' && this.filtroTopico) {
      return this.anotacoesSemSessao.filter(a => a.topicoId === this.filtroTopico);
    }
    
    return this.anotacoesSemSessao;
  }

  getMetasFiltered(): any[] {
    if (!this.detalheDia) return [];
    
    if (this.visualizacaoAtiva === 'categoria' && this.filtroCategoria) {
      return this.detalheDia.metas.filter(m => m.categoriaMateria === this.filtroCategoria);
    }
    
    if (this.visualizacaoAtiva === 'materia' && this.filtroMateria) {
      return this.detalheDia.metas.filter(m => m.materiaId === this.filtroMateria);
    }
    
    return this.detalheDia.metas;
  }

  // Toggle para expandir/colapsar sessões
  toggleSessao(sessao: SessaoAgrupada): void {
    sessao.expandido = !sessao.expandido;
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
    return this.calendarioService.formatarStatus(status);
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

  // Método para obter a cor de uma categoria (para exibição visual)
  getCorCategoria(categoria: string): string {
    if (!this.detalheDia) return '#808080'; // Cinza padrão
    
    const categoriaObj = this.detalheDia.categorias.find(c => c.nome === categoria);
    return categoriaObj?.cor || '#808080';
  }

  // Método para obter a cor de uma matéria
  getCorMateria(materiaId: number): string {
    if (!this.detalheDia) return '#808080';
    
    const materiaObj = this.detalheDia.materias.find(m => m.id === materiaId);
    return materiaObj?.cor || '#808080';
  }
  
  // Calcular percentual de tempo por categoria para gráficos
  getPercentualTempoCategoria(categoriaId: string): number {
    if (!this.detalheDia || this.detalheDia.totalMinutos === 0) return 0;
    
    const tempoDaCategoria = this.tempoTotalPorCategoria[categoriaId] || 0;
    return Math.round((tempoDaCategoria / this.detalheDia.totalMinutos) * 100);
  }
  
  // Obter ícone para tendência de estudo
  getIconeTendencia(): string {
    switch (this.tendenciaEstudo) {
      case 'subindo': return 'fa fa-arrow-up';
      case 'descendo': return 'fa fa-arrow-down';
      default: return 'fa fa-equals';
    }
  }
  
  
  // Obter classe CSS para tendência de estudo
  getClasseTendencia(): string {
    switch (this.tendenciaEstudo) {
      case 'subindo': return 'tendencia-subindo';
      case 'descendo': return 'tendencia-descendo';
      default: return 'tendencia-estavel';
    }
  }

  // Método para calcular percentual de tempo por matéria
getPercentualTempoMateria(materiaId: number): number {
  if (!this.detalheDia || this.detalheDia.totalMinutos === 0) return 0;
  
  const materia = this.detalheDia.materias.find(m => m.id === materiaId);
  const tempoDaMateria = materia?.tempoEstudo || 0;
  return Math.round((tempoDaMateria / this.detalheDia.totalMinutos) * 100);
}

depurarDados(): void {
  if (!this.detalheDia) return;
  
  console.log('Detalhes do dia:', this.detalheDia);
  console.log('Categorias:', this.detalheDia.categorias);
  console.log('Matérias:', this.detalheDia.materias);
  console.log('Tópicos:', this.detalheDia.topicos);
  console.log('Sessões agrupadas:', this.sessoesAgrupadas);
}

  semDadosParaExibir(): boolean {
    const nenhumDadoFiltrado = 
      this.getSessoesFiltered().length === 0 && 
      this.getMetasFiltered().length === 0 && 
      this.getAnotacoesFiltered().length === 0;
    
    const nenhumDadoTotal = 
      !this.detalheDia || 
      (this.detalheDia.sessoes.length === 0 && 
       this.detalheDia.metas.length === 0 && 
       (!this.detalheDia.anotacoes || this.detalheDia.anotacoes.length === 0));
    
    return nenhumDadoFiltrado || nenhumDadoTotal;
  }

  // Método para reconstruir os dados a partir das sessões
reconstruirDadosAgrupados(): void {
  if (!this.detalheDia || !this.detalheDia.sessoes || this.detalheDia.sessoes.length === 0) return;
  
  // Mapas para armazenar dados únicos por ID
  const categoriasMap = new Map();
  const materiasMap = new Map();
  const topicosMap = new Map();
  
  // Percorrer as sessões para extrair dados corretos
  this.detalheDia.sessoes.forEach(sessao => {
    // Processar categoria
    if (sessao.categoriaId && sessao.nomeCategoria) {
      if (!categoriasMap.has(sessao.categoriaId)) {
        categoriasMap.set(sessao.categoriaId, {
          id: sessao.categoriaId,
          nome: sessao.nomeCategoria,
          tempoEstudo: 0,
          cor: sessao.categoriaCor || '#4263eb'
        });
      }
      
      // Acumular tempo de estudo na categoria
      const categoria = categoriasMap.get(sessao.categoriaId);
      categoria.tempoEstudo += sessao.duracao;
    }
    
    // Processar matéria
    if (sessao.materiaId && sessao.nomeMateria) {
      if (!materiasMap.has(sessao.materiaId)) {
        materiasMap.set(sessao.materiaId, {
          id: sessao.materiaId,
          nome: sessao.nomeMateria,
          tempoEstudo: 0,
          categoria: sessao.nomeCategoria || 'Sem categoria',
          cor: sessao.materiaCor || '#4263eb'
        });
      }
      
      // Acumular tempo de estudo na matéria
      const materia = materiasMap.get(sessao.materiaId);
      materia.tempoEstudo += sessao.duracao;
    }
    
    // Processar tópico
    if (sessao.topicoId && sessao.nomeTopico) {
      if (!topicosMap.has(sessao.topicoId)) {
        topicosMap.set(sessao.topicoId, {
          id: sessao.topicoId,
          nome: sessao.nomeTopico,
          materiaId: sessao.materiaId,
          materiaNome: sessao.nomeMateria || 'Sem matéria',
          tempoEstudo: 0
        });
      }
      
      // Acumular tempo de estudo no tópico
      const topico = topicosMap.get(sessao.topicoId);
      topico.tempoEstudo += sessao.duracao;
    }
  });
  
  // Atualizar os arrays no detalheDia com os dados reconstruídos
  this.detalheDia.categorias = Array.from(categoriasMap.values());
  this.detalheDia.materias = Array.from(materiasMap.values());
  this.detalheDia.topicos = Array.from(topicosMap.values());
  
  // Recalcular tempo total por categoria
  this.calcularTempoTotalPorCategoria();
}
}


