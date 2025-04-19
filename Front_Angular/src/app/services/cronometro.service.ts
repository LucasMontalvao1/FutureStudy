import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../environments/environment';
import { Categoria } from '../models/categoria.model';
import { Materia } from '../models/materia.model';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { AnotacoesService } from './anotacoes.service';
import { SessaoEstudo, PausaSessao } from '../models/sessao-estudo.models';
import { AnotacaoSessao } from '../models/anotacao.models';

export interface SessaoEstudoRequestDto {
  categoriaId: number,
  materiaId: number;
  topicoId?: number | null;
}

export interface PausaResponseDto {
  id: number;
  dataInicio: Date;
  dataFim?: Date;
}

@Injectable({
  providedIn: 'root'
})
export class CronometroService extends ApiService {
  private apiUrl = environment.endpoints.sessaoestudo;
  private apiCategoriasUrl = environment.endpoints.categorias;
  private apiMateriasUrl = environment.endpoints.materias;

  // BehaviorSubjects para componentes observarem mudanças
  private tempoSubject = new BehaviorSubject<string>('00:00:00');
  private sessaoAtualSubject = new BehaviorSubject<SessaoEstudo | null>(null);
  private pausaAtualSubject = new BehaviorSubject<PausaSessao | null>(null);
  private categoriasSubject = new BehaviorSubject<Categoria[]>([]);
  private materiasSubject = new BehaviorSubject<Materia[]>([]);
  private materiasFiltradas = new BehaviorSubject<Materia[]>([]);
  private carregandoSubject = new BehaviorSubject<boolean>(false);

  // Observables públicos
  tempo$ = this.tempoSubject.asObservable();
  sessaoAtual$ = this.sessaoAtualSubject.asObservable();
  categorias$ = this.categoriasSubject.asObservable();
  materias$ = this.materiasSubject.asObservable();
  materiasFiltradas$ = this.materiasFiltradas.asObservable();
  carregando$ = this.carregandoSubject.asObservable();

  // Propriedades do cronômetro
  private intervalId: any;
  private segundosDecorridos: number = 0;
  
  constructor(
    protected override http: HttpClient,
    public override authService: AuthService,
    private anotacoesService: AnotacoesService,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    super(http, authService, platformId);
  }

  // Método para carregar categorias do backend
  carregarCategorias(): void {
    if (!this.isBrowser) return;

    this.carregandoSubject.next(true);
    
    this.http.get<Categoria[]>(`${this.apiCategoriasUrl}`, {
      headers: this.getHeaders()
    }).pipe(
      tap((categorias: Categoria[]) => {
        console.log('Categorias carregadas com sucesso:', categorias.length);
        this.categoriasSubject.next(categorias);
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarCategorias', []))
    ).subscribe();
  }

  carregarMaterias(): void {
    if (!this.isBrowser) return;

    this.carregandoSubject.next(true);
    
    this.http.get<Materia[]>(`${this.apiMateriasUrl}`, {
      headers: this.getHeaders()
    }).pipe(
      tap((materias: Materia[]) => {
        console.log('Matérias carregadas com sucesso:', materias.length);
        this.materiasSubject.next(materias);
        this.materiasFiltradas.next(materias); 
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarMaterias', []))
    ).subscribe();
  }

  carregarMateriasPorCategoria(categoriaId: number): void {
    if (!this.isBrowser) return;

    this.carregandoSubject.next(true);
    
    this.http.get<Materia[]>(`${this.apiMateriasUrl}/categoria/${categoriaId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap((materias: Materia[]) => {
        console.log(`Matérias da categoria ${categoriaId} carregadas:`, materias.length);
        this.materiasFiltradas.next(materias);
        this.carregandoSubject.next(false);
      }),
      catchError((error) => {
        console.error(`Erro ao carregar matérias da categoria ${categoriaId}:`, error);
        this.carregandoSubject.next(false);
        
        this.filtrarMateriasPorCategoria(categoriaId);
        
        return this.handleError('carregarMateriasPorCategoria', [])(error);
      })
    ).subscribe();
  }

  filtrarMateriasPorCategoria(categoriaId: number): void {
    if (!this.isBrowser) return;
    
    const todasMaterias = this.materiasSubject.value;
    const materiasFiltradas = todasMaterias.filter(materia => 
      materia.categoria_id === categoriaId || materia.categoriaId === categoriaId
    );
    
    console.log(`Filtradas localmente ${materiasFiltradas.length} matérias para categoria ${categoriaId}`);
    this.materiasFiltradas.next(materiasFiltradas);
  }

  iniciarSessao(request: SessaoEstudoRequestDto): Observable<SessaoEstudo> {
    if (!this.isBrowser) return new Observable<SessaoEstudo>();
    
    this.carregandoSubject.next(true);
    
    return this.http.post<SessaoEstudo>(`${this.apiUrl}`, request, {
      headers: this.getHeaders()
    }).pipe(
      tap((resposta: SessaoEstudo) => {
        console.log('Sessão iniciada:', resposta);
        this.sessaoAtualSubject.next(resposta);
        this.iniciarCronometro(); 
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError<SessaoEstudo>('iniciarSessao'))
    );
  }

  pausarSessao(sessaoId: number): Observable<PausaResponseDto> {
    if (!this.isBrowser) return new Observable<PausaResponseDto>();
    
    this.carregandoSubject.next(true);
    this.pausarCronometro();
    
    const sessaoAtual = this.sessaoAtualSubject.value;
    if (sessaoAtual) {
      sessaoAtual.status = 'pausada';
      sessaoAtual.duracao = Math.floor(this.segundosDecorridos / 60); 
      this.sessaoAtualSubject.next({ ...sessaoAtual });
    }
    
    return this.http.post<PausaResponseDto>(`${this.apiUrl}/${sessaoId}/pausar`, {}, {
      headers: this.getHeaders()
    }).pipe(
      tap((resposta: PausaResponseDto) => {
        console.log('Sessão pausada:', resposta);
        
        const pausa: PausaSessao = {
          id: resposta.id,
          dataInicio: resposta.dataInicio,
          dataFim: resposta.dataFim
        };
        this.pausaAtualSubject.next(pausa);
        
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError<PausaResponseDto>('pausarSessao'))
    );
  }

  retomarSessao(pausaId: number): Observable<any> {
    if (!this.isBrowser) return new Observable<any>();
    
    this.carregandoSubject.next(true);
    
    const sessaoAtual = this.sessaoAtualSubject.value;
    if (sessaoAtual) {
      sessaoAtual.status = 'emAndamento';
      this.sessaoAtualSubject.next({ ...sessaoAtual });
      this.iniciarCronometro(); 
    }
    
    return this.http.post<any>(`${this.apiUrl}/pausas/${pausaId}/retomar`, {}, {
      headers: this.getHeaders()
    }).pipe(
      tap((resposta: any) => {
        console.log('Sessão retomada:', resposta);
        this.pausaAtualSubject.next(null); 
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError<any>('retomarSessao'))
    );
  }

  finalizarSessao(sessaoId: number): Observable<any> {
    if (!this.isBrowser) return new Observable<any>();
    
    this.carregandoSubject.next(true);
    this.pararCronometro();
    
    return this.http.post<any>(`${this.apiUrl}/${sessaoId}/finalizar`, {}, {
      headers: this.getHeaders()
    }).pipe(
      tap((resposta: any) => {
        console.log('Sessão finalizada:', resposta);
        this.sessaoAtualSubject.next(null);
        this.pausaAtualSubject.next(null);
        this.resetarCronometro();
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError<any>('finalizarSessao'))
    );
  }

  salvarNotas(sessaoId: number, conteudo: string): Observable<AnotacaoSessao> {
    if (!this.isBrowser) return new Observable<AnotacaoSessao>();
    
    this.carregandoSubject.next(true);
    
    const sessaoAtual = this.sessaoAtualSubject.value;
    if (!sessaoAtual) {
      console.error('Tentativa de salvar notas sem sessão ativa');
      this.carregandoSubject.next(false);
      return new Observable<AnotacaoSessao>();
    }
    
    const dataAtual = new Date();
    const dataFormatada = `${dataAtual.toLocaleDateString()} ${dataAtual.toLocaleTimeString()}`;
    
    const anotacao = {
      conteudo: conteudo,
      sessaoEstudoId: sessaoId,
      titulo: `Notas da sessão - ${dataFormatada}`
    };
    
    console.log('Salvando notas da sessão:', anotacao);
    
    return this.http.post<AnotacaoSessao>(`${this.apiUrl}/${sessaoId}/anotacoes`, anotacao, {
      headers: this.getHeaders()
    }).pipe(
      tap((resposta: AnotacaoSessao) => {
        console.log('Notas da sessão salvas com sucesso:', resposta);
        this.carregandoSubject.next(false);
      }),
      catchError((error) => {
        console.error('Erro ao salvar notas da sessão:', error);
        this.carregandoSubject.next(false);
        return this.handleError<AnotacaoSessao>('salvarNotas')(error);
      })
    );
  }

  obterAnotacoesDaSessao(sessaoId: number): Observable<AnotacaoSessao[]> {
    if (!this.isBrowser) return new Observable<AnotacaoSessao[]>();
    
    this.carregandoSubject.next(true);
    
    return this.http.get<AnotacaoSessao[]>(`${this.apiUrl}/${sessaoId}/anotacoes`, {
      headers: this.getHeaders()
    }).pipe(
      tap((anotacoes: AnotacaoSessao[]) => {
        console.log(`Obtidas ${anotacoes.length} anotações da sessão ${sessaoId}`);
        this.carregandoSubject.next(false);
      }),
      catchError((error) => {
        console.error(`Erro ao obter anotações da sessão ${sessaoId}:`, error);
        this.carregandoSubject.next(false);
        return this.handleError<AnotacaoSessao[]>('obterAnotacoesDaSessao', [])(error);
      })
    );
  }

  get pausaAtual(): PausaSessao | null {
    return this.pausaAtualSubject.value;
  }

  private iniciarCronometro(): void {
    if (!this.isBrowser) return;
    
    this.atualizarTempoExibido();
    
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
    
    this.intervalId = setInterval(() => {
      this.segundosDecorridos++; 
      this.atualizarTempoExibido();
    }, 1000);
  }

  private pausarCronometro(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private pararCronometro(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
      this.intervalId = null;
    }
  }

  private resetarCronometro(): void {
    this.segundosDecorridos = 0; 
    this.atualizarTempoExibido();
  }

  private atualizarTempoExibido(): void {
    const horas = Math.floor(this.segundosDecorridos / 3600);
    const minutos = Math.floor((this.segundosDecorridos % 3600) / 60);
    const segundos = this.segundosDecorridos % 60;
    
    const horasStr = horas.toString().padStart(2, '0');
    const minutosStr = minutos.toString().padStart(2, '0');
    const segundosStr = segundos.toString().padStart(2, '0');
    
    this.tempoSubject.next(`${horasStr}:${minutosStr}:${segundosStr}`);
  }

  obterSessao(sessaoId: number): Observable<SessaoEstudo> {
    if (!this.isBrowser) return new Observable<SessaoEstudo>();
    
    this.carregandoSubject.next(true);
    
    return this.http.get<SessaoEstudo>(`${this.apiUrl}/${sessaoId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap((sessao: SessaoEstudo) => {
        console.log('Sessão obtida:', sessao);
        this.sessaoAtualSubject.next(sessao);
        this.carregandoSubject.next(false);
      }),
      catchError((error) => {
        console.error(`Erro ao obter sessão ${sessaoId}:`, error);
        this.carregandoSubject.next(false);
        return this.handleError<SessaoEstudo>('obterSessao')(error);
      })
    );
  }
}