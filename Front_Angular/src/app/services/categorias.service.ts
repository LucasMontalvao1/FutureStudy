import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Categoria } from '../models/categoria.model';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { PLATFORM_ID, Inject } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class CategoriasService extends ApiService {
  private apiUrl = environment.endpoints.categorias; // Corrigido para usar o endpoint correto
  
  // BehaviorSubjects para componentes observarem mudanças
  private categoriasSubject = new BehaviorSubject<Categoria[]>([]);
  private carregandoSubject = new BehaviorSubject<boolean>(false);
  
  // Observables públicos
  categorias$ = this.categoriasSubject.asObservable();
  carregando$ = this.carregandoSubject.asObservable();
  
  constructor(
    protected override http: HttpClient,
    protected override authService: AuthService,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    super(http, authService, platformId);
  }
  
  // Carregar todas as categorias
  carregarCategorias(): void {
    if (!this.isBrowser) return;
    
    this.carregandoSubject.next(true);
    
    this.http.get<Categoria[]>(`${this.apiUrl}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(categorias => {
        this.categoriasSubject.next(categorias);
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarCategorias', []))
    ).subscribe();
  }
  
  // Obter uma categoria específica por ID
  obterCategoria(id: number): Observable<Categoria> {
    if (!this.isBrowser) return new Observable<Categoria>();
    
    return this.http.get<Categoria>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      catchError(this.handleError<Categoria>(`obterCategoria(${id})`))
    );
  }
  
  // Criar uma nova categoria
  criarCategoria(categoria: Categoria): Observable<Categoria> {
    if (!this.isBrowser) return new Observable<Categoria>();
    
    return this.http.post<Categoria>(`${this.apiUrl}`, categoria, {
      headers: this.getHeaders()
    }).pipe(
      tap(novaCategoria => {
        // Adicionar ao array local
        const categoriasAtuais = this.categoriasSubject.value;
        this.categoriasSubject.next([...categoriasAtuais, novaCategoria]);
      }),
      catchError(this.handleError<Categoria>('criarCategoria'))
    );
  }
  
  // Atualizar uma categoria existente
  atualizarCategoria(id: number, categoria: Partial<Categoria>): Observable<Categoria> {
    if (!this.isBrowser) return new Observable<Categoria>();
    
    return this.http.put<Categoria>(`${this.apiUrl}/${id}`, categoria, {
      headers: this.getHeaders()
    }).pipe(
      tap(categoriaAtualizada => {
        // Atualizar no array local
        const categoriasAtuais = this.categoriasSubject.value;
        const index = categoriasAtuais.findIndex(c => c.id === id);
        if (index !== -1) {
          categoriasAtuais[index] = categoriaAtualizada;
          this.categoriasSubject.next([...categoriasAtuais]);
        }
      }),
      catchError(this.handleError<Categoria>(`atualizarCategoria(${id})`))
    );
  }
  
  // Excluir uma categoria
  excluirCategoria(id: number): Observable<void> {
    if (!this.isBrowser) return new Observable<void>();
    
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(() => {
        // Remover do array local
        const categoriasAtuais = this.categoriasSubject.value;
        this.categoriasSubject.next(categoriasAtuais.filter(c => c.id !== id));
      }),
      catchError(this.handleError<void>(`excluirCategoria(${id})`))
    );
  }
}