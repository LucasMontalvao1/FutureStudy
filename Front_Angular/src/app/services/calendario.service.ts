import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject, of, forkJoin } from 'rxjs';
import { map, catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from './auth.service';
import { MetasService } from './metas.service';
import { AnotacoesService } from './anotacoes.service';

export interface DiaCalendario {
  dia: number;
  minutosEstudados: number;
  totalSessoes?: number;
  metas?: any[];
}

export interface MateriaResumo {
  id: number;
  nome: string;
  tempoEstudo: number;
  categoria?: string;
  cor?: string;
}

export interface TopicoResumo {
  id: number;
  nome: string;
  materiaId: number;
  materiaNome: string;
  tempoEstudo: number;
}

export interface CategoriaResumo {
  id: any;
  nome: string;
  tempoEstudo: number;
  cor?: string;
}

export interface DetalhesDia {
  sessoes: any[];
  totalMinutos: number;
  materias: MateriaResumo[];
  topicos: TopicoResumo[];
  metas: any[];
  anotacoes: any[];
  categorias: CategoriaResumo[];
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
    }).pipe(
      tap(response => {
        console.log(`Obtidos dados do calendário para ${mes}/${ano}:`, response);
      }),
      catchError(error => {
        console.error('Erro ao obter dados do calendário:', error);
        return of([]);
      })
    );
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
    }).pipe(
      tap(response => {
        console.log(`Obtidos detalhes para ${this.formatarData(data)}:`, response);
      }),
      catchError(error => {
        console.error(`Erro ao obter detalhes para ${this.formatarData(data)}:`, error);
        return of([]);
      })
    );
  }

  // Método para atualizar uma sessão
  atualizarSessao(sessao: any): Observable<any> {
    if (!this.isBrowser) {
      return of(null);
    }

    console.log(`Atualizando sessão ${sessao.id}:`, sessao);

    return this.http.put<any>(`${this.apiUrl}/${sessao.id}`, sessao, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Sessão ${sessao.id} atualizada com sucesso:`, response);
      }),
      catchError(error => {
        console.error(`Erro ao atualizar sessão ${sessao.id}:`, error);
        throw error;
      })
    );
  }

  // Método para criar uma nova sessão
  criarSessao(sessao: any): Observable<any> {
    if (!this.isBrowser) {
      return of(null);
    }

    console.log('Criando nova sessão:', sessao);

    return this.http.post<any>(`${this.apiUrl}`, sessao, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log('Sessão criada com sucesso:', response);
      }),
      catchError(error => {
        console.error('Erro ao criar sessão:', error);
        throw error;
      })
    );
  }

  // Método para excluir uma sessão
  excluirSessao(id: number): Observable<any> {
    if (!this.isBrowser) {
      return of(null);
    }

    console.log(`Excluindo sessão ${id}`);

    return this.http.delete<any>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Sessão ${id} excluída com sucesso:`, response);
      }),
      catchError(error => {
        console.error(`Erro ao excluir sessão ${id}:`, error);
        throw error;
      })
    );
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
        if (statusNormalizado === 'em_andamento') statusNormalizado = 'emandamento';
        
        return {
          ...s,
          duracao: minutos,
          duracaoFormatada: this.formatarTempoEstudo(minutos),
          duracaoCalculada: duracaoCalculada,
          statusNormalizado: statusNormalizado,
          // Garantir que todas as sessões tenham uma categoria (mesmo que seja vazia)
          categoriaMateria: s.categoriaMateria || 'Sem categoria'
        };
      }),
      totalMinutos: 0,
      materias: [],
      topicos: [],
      metas: this.processarMetas(metas) || [],
      anotacoes: anotacoes || [],
      categorias: []
    };
  
    // Calcular o total de minutos somando todas as sessões
    detalhes.totalMinutos = detalhes.sessoes.reduce((total, sessao) => total + (sessao.duracao || 0), 0);
  
    // Agrupa por matéria
    const materiasPorId: { [id: number]: MateriaResumo } = {};
    
    // Agrupa por tópico
    const topicosPorId: { [key: string]: TopicoResumo } = {};
    
    // Agrupa por categoria
    const categoriasPorNome: { [nome: string]: CategoriaResumo } = {};
  
    // Processa cada sessão para agrupar
    detalhes.sessoes.forEach(sessao => {
      // Agrupa por matéria
      if (!materiasPorId[sessao.materiaId]) {
        materiasPorId[sessao.materiaId] = {
          id: sessao.materiaId,
          nome: sessao.materiaNome || 'Matéria não especificada',
          tempoEstudo: 0,
          categoria: sessao.categoriaMateria || 'Sem categoria',
          cor: sessao.materiaCor
        };
      }
      materiasPorId[sessao.materiaId].tempoEstudo += sessao.duracao || 0;
      
      // Agrupa por tópico
      if (sessao.topicoId && sessao.topicoNome) {
        const topicoKey = `${sessao.materiaId}-${sessao.topicoId}`;
        if (!topicosPorId[topicoKey]) {
          topicosPorId[topicoKey] = {
            id: sessao.topicoId,
            nome: sessao.topicoNome,
            materiaId: sessao.materiaId,
            materiaNome: sessao.materiaNome || 'Matéria não especificada',
            tempoEstudo: 0
          };
        }
        topicosPorId[topicoKey].tempoEstudo += sessao.duracao || 0;
      }
      
      // Agrupa por categoria
      const categoria = sessao.categoriaMateria || 'Sem categoria';
      if (!categoriasPorNome[categoria]) {
        categoriasPorNome[categoria] = {
          id: categoria.id,
          nome: categoria,
          tempoEstudo: 0,
          cor: sessao.categoriaCor
        };
      }
      categoriasPorNome[categoria].tempoEstudo += sessao.duracao || 0;
    });
  
    // Adiciona categorização para anotações se não existir
    detalhes.anotacoes = detalhes.anotacoes.map(anotacao => {
      return {
        ...anotacao,
        materiaId: anotacao.materiaId || null,
        materiaNome: anotacao.materiaNome || 'Sem matéria',
        topicoId: anotacao.topicoId || null,
        topicoNome: anotacao.topicoNome || null,
        categoriaMateria: anotacao.categoriaMateria || 'Sem categoria'
      };
    });

    // Converte os objetos em arrays
    detalhes.materias = Object.values(materiasPorId);
    detalhes.topicos = Object.values(topicosPorId);
    detalhes.categorias = Object.values(categoriasPorNome);
  
    console.log('Detalhes processados:', detalhes);
    return detalhes;
  }
  
  // Método para processar e normalizar metas
  private processarMetas(metas: any[] = []): any[] {
    return metas.map(meta => {
      // Garantir que todas as metas tenham as propriedades necessárias
      const percentualConcluido = meta.quantidade > 0
        ? Math.round((meta.progresso / meta.quantidade) * 100)
        : 0;
        
      return {
        ...meta,
        percentualConcluido,
        // Garantir que a meta tenha uma categoria (mesmo que seja vazia)
        categoriaMateria: meta.categoriaMateria || 'Sem categoria'
      };
    });
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
      case 'emandamento':
        return 'Em andamento';
      case 'pausada':
        return 'Pausada';
      case 'concluida':
      case 'finalizada':
        return 'Finalizada';
      default:
        return 'Desconhecido';
    }
  }

  // Novos métodos para análise por categoria/matéria/tópico
  getTempoEstudoPorCategoria(dados: DetalhesDia): {nome: string, tempo: number, percentual: number}[] {
    if (!dados || !dados.categorias || dados.categorias.length === 0) {
      return [];
    }

    return dados.categorias.map(cat => {
      return {
        id: cat.id,
        nome: cat.nome,
        tempo: cat.tempoEstudo,
        percentual: dados.totalMinutos > 0 ? (cat.tempoEstudo / dados.totalMinutos) * 100 : 0
      };
    }).sort((a, b) => b.tempo - a.tempo);
  }

  getTempoEstudoPorMateria(dados: DetalhesDia): {id: number, nome: string, tempo: number, percentual: number}[] {
    if (!dados || !dados.materias || dados.materias.length === 0) {
      return [];
    }

    return dados.materias.map(mat => {
      return {
        id: mat.id,
        nome: mat.nome,
        tempo: mat.tempoEstudo,
        percentual: dados.totalMinutos > 0 ? (mat.tempoEstudo / dados.totalMinutos) * 100 : 0
      };
    }).sort((a, b) => b.tempo - a.tempo);
  }

  getTempoEstudoPorTopico(dados: DetalhesDia): {id: number, nome: string, materiaId: number, materiaNome: string, tempo: number, percentual: number}[] {
    if (!dados || !dados.topicos || dados.topicos.length === 0) {
      return [];
    }

    return dados.topicos.map(top => {
      return {
        id: top.id,
        nome: top.nome,
        materiaId: top.materiaId,
        materiaNome: top.materiaNome,
        tempo: top.tempoEstudo,
        percentual: dados.totalMinutos > 0 ? (top.tempoEstudo / dados.totalMinutos) * 100 : 0
      };
    }).sort((a, b) => b.tempo - a.tempo);
  }

  getAnotacoesPorMateria(dados: DetalhesDia): {[materiaId: number]: any[]} {
    if (!dados || !dados.anotacoes || dados.anotacoes.length === 0) {
      return {};
    }

    const resultado: {[materiaId: number]: any[]} = {};
    
    dados.anotacoes.forEach(anotacao => {
      if (anotacao.materiaId) {
        if (!resultado[anotacao.materiaId]) {
          resultado[anotacao.materiaId] = [];
        }
        resultado[anotacao.materiaId].push(anotacao);
      }
    });

    return resultado;
  }

  getAnotacoesPorCategoria(dados: DetalhesDia): {[categoria: string]: any[]} {
    if (!dados || !dados.anotacoes || dados.anotacoes.length === 0) {
      return {};
    }

    const resultado: {[categoria: string]: any[]} = {};
    
    dados.anotacoes.forEach(anotacao => {
      const categoria = anotacao.categoriaMateria || 'Sem categoria';
      if (!resultado[categoria]) {
        resultado[categoria] = [];
      }
      resultado[categoria].push(anotacao);
    });

    return resultado;
  }
}