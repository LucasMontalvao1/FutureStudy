import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Topico } from '../models/topico.model';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { PLATFORM_ID, Inject } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TopicosService extends ApiService {
  private apiUrl = environment.endpoints.topicos; // Corrigido para usar o endpoint correto
  
  // BehaviorSubjects para componentes observarem mudanças
  private topicosSubject = new BehaviorSubject<Topico[]>([]);
  private topicosFiltradosSubject = new BehaviorSubject<Topico[]>([]);
  private carregandoSubject = new BehaviorSubject<boolean>(false);
  
  // Observables públicos
  topicos$ = this.topicosSubject.asObservable();
  topicosFiltrados$ = this.topicosFiltradosSubject.asObservable();
  carregando$ = this.carregandoSubject.asObservable();
  
  constructor(
    protected override http: HttpClient,
    protected override authService: AuthService,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    super(http, authService, platformId);
  }
  
  // Carregar todos os tópicos
  carregarTopicos(): void {
    if (!this.isBrowser) return;
    
    this.carregandoSubject.next(true);
    
    this.http.get<Topico[]>(`${this.apiUrl}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(topicos => {
        this.topicosSubject.next(topicos);
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarTopicos', []))
    ).subscribe();
  }
  
  // Carregar tópicos por matéria
  carregarTopicosPorMateria(materiaId: number): void {
    if (!this.isBrowser) return;
    
    this.carregandoSubject.next(true);
    console.log(`Buscando tópicos para matéria ID: ${materiaId}`);
    
    // URL atualizada para buscar tópicos por matéria
    this.http.get<any[]>(`${this.apiUrl}/materia/${materiaId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(topicos => {
        console.log('Resposta da API:', topicos);
        // Normalizar cada tópico retornado
        const topicosNormalizados = topicos.map(t => this.normalizarTopico(t));
        console.log('Tópicos normalizados:', topicosNormalizados);
        this.topicosFiltradosSubject.next(topicosNormalizados);
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarTopicosPorMateria', []))
    ).subscribe();
  }
  
  filtrarTopicosPorMateria(materiaId: number): void {
    if (!this.isBrowser) return;
    
    console.log(`Filtrando tópicos localmente para matéria ID: ${materiaId}`);
    // Primeiro, tente carregar da API
    this.carregarTopicosPorMateria(materiaId);
    
    // Também filtre localmente (caso de fallback)
    const todosTopicos = this.topicosSubject.value;
    console.log('Todos os tópicos disponíveis para filtrar:', todosTopicos);
    
    const topicosFiltrados = todosTopicos.filter(
      topico => (topico.materia_id === materiaId || topico.materiaId === materiaId)
    );
    console.log('Tópicos filtrados localmente:', topicosFiltrados);
    
    if (topicosFiltrados.length > 0) {
      this.topicosFiltradosSubject.next(topicosFiltrados);
    }
  }
  
  // Obter um tópico específico por ID
  obterTopico(id: number): Observable<Topico> {
    if (!this.isBrowser) return new Observable<Topico>();
    
    return this.http.get<Topico>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError<Topico>(`obterTopico(${id})`))
    );
  }
  
  // Criar um novo tópico
  criarTopico(topico: any): Observable<Topico> {
    if (!this.isBrowser) return new Observable<Topico>();
    
    // Adaptar o formato para o que a API espera
    const topicoParaAPI = {
      usuarioId: topico.usuario_id || 1, // Assumindo usuário ID 1 se não fornecido
      materiaId: topico.materia_id,
      nome: topico.nome
    };
    
    console.log('Dados formatados para enviar à API:', topicoParaAPI);
    
    return this.http.post<any>(`${this.apiUrl}`, topicoParaAPI, {
      headers: this.getHeaders()
    }).pipe(
      tap(novoTopico => {
        console.log('Resposta da API ao criar:', novoTopico);
        // Normalizar o tópico retornado
        const topicoNormalizado = this.normalizarTopico(novoTopico);
        
        // Adicionar ao array local
        const topicosAtuais = this.topicosSubject.value;
        this.topicosSubject.next([...topicosAtuais, topicoNormalizado]);
        
        // Atualizar tópicos filtrados se necessário
        if (topicoNormalizado.materia_id === this.getMateriaIdFiltrado()) {
          const filtrados = this.topicosFiltradosSubject.value;
          this.topicosFiltradosSubject.next([...filtrados, topicoNormalizado]);
        }
      }),
      catchError(this.handleError<Topico>('criarTopico'))
    );
  }
  
  // Método auxiliar para obter a matéria ID atualmente filtrada
  private getMateriaIdFiltrado(): number | null {
    const topicosFiltrados = this.topicosFiltradosSubject.value;
    if (topicosFiltrados.length > 0) {
      return (topicosFiltrados[0].materia_id || topicosFiltrados[0].materiaId || null);
    }
    return null;
  }


  // Atualizar um tópico existente
  atualizarTopico(id: number, topico: Partial<Topico>): Observable<Topico> {
    if (!this.isBrowser) return new Observable<Topico>();
    
    return this.http.put<Topico>(`${this.apiUrl}/${id}`, topico, {
      headers: this.getHeaders()
    }).pipe(
      tap(topicoAtualizado => {
        // Atualizar no array local
        const topicosAtuais = this.topicosSubject.value;
        const index = topicosAtuais.findIndex(t => t.id === id);
        if (index !== -1) {
          topicosAtuais[index] = topicoAtualizado;
          this.topicosSubject.next([...topicosAtuais]);
          
          // Atualizar tópicos filtrados se necessário
          const materiaId = topicoAtualizado.materia_id || topicoAtualizado.materiaId;
          if (materiaId !== undefined) {
            this.filtrarTopicosPorMateria(materiaId);
          }
        }
      }),
      catchError(this.handleError<Topico>(`atualizarTopico(${id})`))
    );
  }

  limparFiltros(): void {
    console.log('Método limparFiltros chamado');
    if (!this.isBrowser) return;
    
    // Atualizar a lista de tópicos filtrados com todos os tópicos
    const todosTopicos = this.topicosSubject.value;
    console.log('Todos os tópicos disponíveis:', todosTopicos);
    this.topicosFiltradosSubject.next([...todosTopicos]);
  }

  private normalizarTopico(topico: any): Topico {
    return {
      id: topico.id,
      usuario_id: topico.usuarioId || topico.usuario_id,
      materia_id: topico.materiaId || topico.materia_id,
      usuarioId: topico.usuarioId || topico.usuario_id,
      materiaId: topico.materiaId || topico.materia_id,
      nome: topico.nome,
      criado_em: topico.criadoEm || topico.criado_em,
      atualizado_em: topico.atualizadoEm || topico.atualizado_em,
      criadoEm: topico.criadoEm || topico.criado_em,
      atualizadoEm: topico.atualizadoEm || topico.atualizado_em
    };
  }
  
  // Excluir um tópico
  excluirTopico(id: number): Observable<void> {
    if (!this.isBrowser) return new Observable<void>();
    
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(() => {
        // Remover do array local
        const topicosAtuais = this.topicosSubject.value;
        const topicosFiltrados = this.topicosFiltradosSubject.value;
        
        this.topicosSubject.next(topicosAtuais.filter(t => t.id !== id));
        this.topicosFiltradosSubject.next(topicosFiltrados.filter(t => t.id !== id));
      }),
      catchError(this.handleError<void>(`excluirTopico(${id})`))
    );
  }
}
