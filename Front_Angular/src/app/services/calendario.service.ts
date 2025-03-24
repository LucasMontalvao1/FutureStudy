import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of, forkJoin } from 'rxjs';
import { map } from 'rxjs/operators'; // Adicionando o import do map
import { environment } from '../../environments/environment';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from './auth.service';
import { MetasService } from './metas.service';
import { AnotacoesService } from './anotacoes.service';

export interface DiaCalendario {
  dia: number;
  minutosEstudados: number;
}

export interface DetalhesDia {
  sessoes: any[];
  totalMinutos: number;
  materias: { nome: string; tempoEstudo: number }[];
  metas: any[];
  anotacoes: any[];
}

@Injectable({
  providedIn: 'root'
})
export class CalendarioService {
  private apiUrl = environment.endpoints.sessaoestudo;
  private isBrowser: boolean;

  // Subjects para observables
  private _mesSelecionado = new BehaviorSubject<Date>(new Date());
  private _diaSelecionado = new BehaviorSubject<Date>(new Date());

  // Observables públicos
  public mesSelecionado$ = this._mesSelecionado.asObservable();
  public diaSelecionado$ = this._diaSelecionado.asObservable();

  constructor(
    private http: HttpClient,
    private authService: AuthService,
    private metasService: MetasService,
    private anotacoesService: AnotacoesService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(this.platformId);
  }

  // Método privado para obter headers com o token
  private getHeaders(): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    // Só acessa sessionStorage se estiver no navegador
    if (this.isBrowser) {
      const token = this.authService.getToken();
      if (token) {
        headers = headers.set('Authorization', `Bearer ${token}`);
      }
    }

    return headers;
  }

  // Obter dados do calendário por mês e ano
  getCalendario(mes: number, ano: number): Observable<DiaCalendario[]> {
    // Se não estiver no navegador, retorna um array vazio
    if (!this.isBrowser) {
      return of([]);
    }

    const params = new HttpParams()
      .append('mes', mes.toString())
      .append('ano', ano.toString());

    return this.http.get<DiaCalendario[]>(`${this.apiUrl}/calendario`, {
      headers: this.getHeaders(),
      params
    });
  }

  // Modificar esta função para retornar Observable<any[]> em vez de DetalhesDia
  getDetalhesDia(data: Date): Observable<any[]> {
    // Se não estiver no navegador, retorna um array vazio
    if (!this.isBrowser) {
      return of([]);
    }

    // Início e fim do dia (para obter sessões)
    const inicio = new Date(data);
    inicio.setHours(0, 0, 0, 0);
    
    const fim = new Date(data);
    fim.setHours(23, 59, 59, 999);

    // Parâmetros para buscar sessões
    const params = new HttpParams()
      .append('inicio', inicio.toISOString())
      .append('fim', fim.toISOString());

    // Retornamos apenas as sessões como array para manter a compatibilidade
    return this.http.get<any[]>(`${this.apiUrl}`, {
      headers: this.getHeaders(),
      params
    });
  }

  processarDetalhes(sessoes: any[] = [], metas: any[] = [], anotacoes: any[] = []): DetalhesDia {
    console.log('Sessões recebidas:', JSON.stringify(sessoes, null, 2));
    
    // Inicializa o objeto de detalhes
    const detalhes: DetalhesDia = {
      sessoes: sessoes.map(s => {
        // Calcular duração com base nas datas, se disponíveis
        let minutos = 0;
        let duracaoCalculada = false;
        
        if (s.dataInicio && s.dataFim) {
          const inicio = new Date(s.dataInicio);
          const fim = new Date(s.dataFim);
          // Verificar se as datas são válidas
          if (!isNaN(inicio.getTime()) && !isNaN(fim.getTime())) {
            const diferencaMs = fim.getTime() - inicio.getTime();
            if (diferencaMs > 0) {
              minutos = Math.max(1, Math.floor(diferencaMs / (1000 * 60)));
              duracaoCalculada = true;
              console.log(`Sessão de ${inicio} até ${fim}: ${minutos} minutos`);
            }
          }
        }
        
        // Normalizar o status para garantir consistência
        let statusNormalizado = s.status ? s.status.toLowerCase() : '';
        if (statusNormalizado === 'concluida') statusNormalizado = 'finalizada';
        
        return {
          ...s,
          duracao: minutos,
          duracaoFormatada: this.formatarTempoEstudo(minutos),
          duracaoCalculada: duracaoCalculada,
          statusNormalizado: statusNormalizado
        };
      }),
      totalMinutos: 0,
      materias: [],
      metas: metas || [],
      anotacoes: anotacoes || []
    };
  
    // Calcular o total de minutos somando todas as sessões
    detalhes.totalMinutos = detalhes.sessoes.reduce((total, sessao) => total + (sessao.duracao || 0), 0);
  
    // Agrupa por matéria
    const materiasPorId: { [id: number]: { nome: string, tempoEstudo: number } } = {};
  
    // Processa cada sessão para agrupar por matéria
    detalhes.sessoes.forEach(sessao => {
      // Agrupa por matéria
      if (!materiasPorId[sessao.materiaId]) {
        materiasPorId[sessao.materiaId] = {
          nome: sessao.materiaNome || 'Matéria não especificada',
          tempoEstudo: 0
        };
      }
      materiasPorId[sessao.materiaId].tempoEstudo += sessao.duracao || 0;
    });
  
    // Converte o objeto de matérias em array
    detalhes.materias = Object.values(materiasPorId);
  
    console.log('Detalhes processados:', detalhes);
    return detalhes;
  }
  
  // Método para formatar data como YYYY-MM-DD
  formatarData(data: Date): string {
    const ano = data.getFullYear();
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const dia = String(data.getDate()).padStart(2, '0');
    return `${ano}-${mes}-${dia}`;
  }

  // Métodos para gerenciar a navegação do calendário
  selecionarMes(data: Date): void {
    // Cria uma nova data com o primeiro dia do mês
    const novaMes = new Date(data.getFullYear(), data.getMonth(), 1);
    this._mesSelecionado.next(novaMes);
  }

  selecionarDia(data: Date): void {
    this._diaSelecionado.next(data);
  }

  voltarMes(): void {
    const mesAtual = this._mesSelecionado.value;
    const novoMes = new Date(mesAtual.getFullYear(), mesAtual.getMonth() - 1, 1);
    this._mesSelecionado.next(novoMes);
  }

  avancarMes(): void {
    const mesAtual = this._mesSelecionado.value;
    const novoMes = new Date(mesAtual.getFullYear(), mesAtual.getMonth() + 1, 1);
    this._mesSelecionado.next(novoMes);
  }

  // Método para formatar o tempo de estudo
  formatarTempoEstudo(minutos: number): string {
    if (!minutos || isNaN(minutos) || minutos <= 0) return '0min';
    
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

  // Formata o status da sessão para exibição
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
}