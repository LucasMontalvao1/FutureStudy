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
    
    // URL específica para buscar tópicos por matéria conforme sua API
    this.http.get<Topico[]>(`${this.apiUrl}/materia/${materiaId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(topicos => {
        this.topicosFiltradosSubject.next(topicos);
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarTopicosPorMateria', []))
    ).subscribe();
  }
  
  // Filtrar tópicos por matéria (usando dados já carregados)
  filtrarTopicosPorMateria(materiaId: number): void {
    if (!this.isBrowser) return;
    
    const todosTopicos = this.topicosSubject.value;
    const topicosFiltrados = todosTopicos.filter(
      topico => topico.materia_id === materiaId || topico.materiaId === materiaId
    );
    this.topicosFiltradosSubject.next(topicosFiltrados);
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
  criarTopico(topico: Topico): Observable<Topico> {
    if (!this.isBrowser) return new Observable<Topico>();
    
    return this.http.post<Topico>(`${this.apiUrl}`, topico, {
      headers: this.getHeaders()
    }).pipe(
      tap(novoTopico => {
        // Adicionar ao array local
        const topicosAtuais = this.topicosSubject.value;
        this.topicosSubject.next([...topicosAtuais, novoTopico]);
        
        // Atualizar tópicos filtrados se necessário
        if (novoTopico.materia_id === topicosAtuais[0]?.materia_id) {
          const filtrados = this.topicosFiltradosSubject.value;
          this.topicosFiltradosSubject.next([...filtrados, novoTopico]);
        }
      }),
      catchError(this.handleError<Topico>('criarTopico'))
    );
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
          this.filtrarTopicosPorMateria(topicoAtualizado.materia_id);
        }
      }),
      catchError(this.handleError<Topico>(`atualizarTopico(${id})`))
    );
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
