import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Materia } from '../models/materia.model';
import { ApiService } from './api.service';
import { AuthService } from './auth.service';
import { PLATFORM_ID, Inject } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class MateriasService extends ApiService {
  private apiUrl = environment.endpoints.materias;
  
  // BehaviorSubjects para componentes observarem mudanças
  private materiasSubject = new BehaviorSubject<Materia[]>([]);
  private materiasFiltradosSubject = new BehaviorSubject<Materia[]>([]);
  private carregandoSubject = new BehaviorSubject<boolean>(false);
  
  // Observables públicos
  materias$ = this.materiasSubject.asObservable();
  materiasFiltrados$ = this.materiasFiltradosSubject.asObservable();
  carregando$ = this.carregandoSubject.asObservable();
  
  // Categoria atual sendo filtrada
  private categoriaAtualId: number | null = null;
  
  constructor(
    protected override http: HttpClient,
    protected override authService: AuthService,
    @Inject(PLATFORM_ID) platformId: Object
  ) {
    super(http, authService, platformId);
    this.isBrowser = isPlatformBrowser(platformId);
  }
  
  // Carregar todas as matérias
  carregarMaterias(): void {
    if (!this.isBrowser) return;
    
    this.carregandoSubject.next(true);
    
    this.http.get<Materia[]>(`${this.apiUrl}`, {
      headers: this.getHeaders()
    }).pipe(
      tap((materias: Materia[]) => {
        console.log('Matérias carregadas com sucesso:', materias.length);
        this.materiasSubject.next(materias);
        
        // Reaplica filtro se houver um filtro ativo
        if (this.categoriaAtualId !== null) {
          this.filtrarMateriasPorCategoria(this.categoriaAtualId);
        } else {
          this.materiasFiltradosSubject.next(materias);
        }
        
        this.carregandoSubject.next(false);
      }),
      catchError(this.handleError('carregarMaterias', []))
    ).subscribe();
  }
  
  // Carregar matérias por categoria
  carregarMateriasPorCategoria(categoriaId: number): void {
    if (!this.isBrowser) return;
    
    this.carregandoSubject.next(true);
    this.categoriaAtualId = categoriaId;
    
    this.http.get<Materia[]>(`${this.apiUrl}/categoria/${categoriaId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap((materias: Materia[]) => {
        console.log(`Matérias da categoria ${categoriaId} carregadas com sucesso:`, materias.length);
        // Atualizar apenas as matérias filtradas, não todas as matérias
        this.materiasFiltradosSubject.next(materias);
        this.carregandoSubject.next(false);
      }),
      catchError((error) => {
        console.error(`Erro ao carregar matérias da categoria ${categoriaId}:`, error);
        this.carregandoSubject.next(false);
        
        // Em caso de erro, tentar filtragem local
        this.filtrarMateriasPorCategoriaLocal(categoriaId);
        
        return this.handleError('carregarMateriasPorCategoria', [])(error);
      })
    ).subscribe();
  }
  
  // Filtrar matérias por categoria usando o endpoint específico
  filtrarMateriasPorCategoria(categoriaId: number | null): void {
    if (!this.isBrowser) return;
    
    this.categoriaAtualId = categoriaId;
    
    if (categoriaId === null) {
      // Se categoriaId for null, retorna todas as matérias
      this.materiasFiltradosSubject.next(this.materiasSubject.value);
      return;
    }
    
    // Usar o novo endpoint para buscar matérias por categoria
    this.carregarMateriasPorCategoria(categoriaId);
  }
  
  // Filtrar matérias por categoria localmente (fallback)
  private filtrarMateriasPorCategoriaLocal(categoriaId: number): void {
    const todasMaterias = this.materiasSubject.value;
    
    // Garante que estamos checando todos os formatos possíveis de ID de categoria
    const materiasFiltradas = todasMaterias.filter(
      materia => materia.categoriaId === categoriaId || materia.categoria_id === categoriaId
    );
    
    console.log(`Filtradas localmente ${materiasFiltradas.length} matérias para categoria ${categoriaId}`);
    this.materiasFiltradosSubject.next(materiasFiltradas);
  }
  
  // Limpar filtros
  limparFiltros(): void {
    if (!this.isBrowser) return;
    
    this.categoriaAtualId = null;
    this.materiasFiltradosSubject.next(this.materiasSubject.value);
  }
  
  // Obter uma matéria específica por ID
  obterMateria(id: number): Observable<Materia> {
    if (!this.isBrowser) return new Observable<Materia>();
    
    console.log(`Obtendo matéria ID: ${id}`);
    return this.http.get<Materia>(`${this.apiUrl}/${id}`, { 
      headers: this.getHeaders()
    }).pipe(
      tap((materia: Materia) => console.log(`Matéria obtida: ${materia.id} - ${materia.nome}`)),
      catchError(this.handleError<Materia>(`obterMateria(${id})`))
    );
  }
  
  // Criar uma nova matéria
  criarMateria(materia: Materia): Observable<Materia> {
    if (!this.isBrowser) return new Observable<Materia>();
    
    console.log('Criando nova matéria:', materia);
    return this.http.post<Materia>(`${this.apiUrl}`, materia, {
      headers: this.getHeaders()
    }).pipe(
      tap((novaMateria: Materia) => {
        console.log('Matéria criada com sucesso:', novaMateria);
        
        // Adicionar ao array local
        const materiasAtuais = this.materiasSubject.value;
        this.materiasSubject.next([...materiasAtuais, novaMateria]);
        
        // Atualizar matérias filtradas se a nova matéria pertence à categoria atual
        if (this.categoriaAtualId === null || 
            novaMateria.categoriaId === this.categoriaAtualId || 
            novaMateria.categoria_id === this.categoriaAtualId) {
          const materiasFiltradas = this.materiasFiltradosSubject.value;
          this.materiasFiltradosSubject.next([...materiasFiltradas, novaMateria]);
        }
      }),
      catchError(this.handleError<Materia>('criarMateria'))
    );
  }
  
  // Atualizar uma matéria existente
  atualizarMateria(id: number, materia: Partial<Materia>): Observable<Materia> {
    if (!this.isBrowser) return new Observable<Materia>();
    
    console.log(`Atualizando matéria ${id}:`, materia);
    return this.http.put<Materia>(`${this.apiUrl}/${id}`, materia, {
      headers: this.getHeaders()
    }).pipe(
      tap((materiaAtualizada: Materia) => {
        console.log('Matéria atualizada com sucesso:', materiaAtualizada);
        
        // Atualizar no array local
        const materiasAtuais = this.materiasSubject.value;
        const index = materiasAtuais.findIndex(m => m.id === id);
        
        if (index !== -1) {
          materiasAtuais[index] = materiaAtualizada;
          this.materiasSubject.next([...materiasAtuais]);
          
          // Verificar se a categoria mudou e atualizar os filtros
          if (this.categoriaAtualId !== null) {
            // Verificando ambos os formatos possíveis de ID de categoria
            const pertenceCategoria = materiaAtualizada.categoriaId === this.categoriaAtualId || 
                                     materiaAtualizada.categoria_id === this.categoriaAtualId;
            
            // Se estava na categoria filtrada mas agora não está mais, remover das filtradas
            if (!pertenceCategoria) {
              const materiasFiltradas = this.materiasFiltradosSubject.value;
              this.materiasFiltradosSubject.next(
                materiasFiltradas.filter(m => m.id !== id)
              );
            } 
            // Se não estava na categoria filtrada mas agora está, adicionar às filtradas
            else {
              const materiasFiltradas = this.materiasFiltradosSubject.value;
              const jaExiste = materiasFiltradas.some(m => m.id === id);
              
              if (!jaExiste) {
                this.materiasFiltradosSubject.next([...materiasFiltradas, materiaAtualizada]);
              } else {
                // Apenas atualiza a existente
                const indexFiltrado = materiasFiltradas.findIndex(m => m.id === id);
                materiasFiltradas[indexFiltrado] = materiaAtualizada;
                this.materiasFiltradosSubject.next([...materiasFiltradas]);
              }
            }
          } else {
            // Se não há filtro, atualiza a lista filtrada também
            const materiasFiltradas = this.materiasFiltradosSubject.value;
            const indexFiltrado = materiasFiltradas.findIndex(m => m.id === id);
            
            if (indexFiltrado !== -1) {
              materiasFiltradas[indexFiltrado] = materiaAtualizada;
              this.materiasFiltradosSubject.next([...materiasFiltradas]);
            }
          }
        }
      }),
      catchError(this.handleError<Materia>(`atualizarMateria(${id})`))
    );
  }
  
  // Excluir uma matéria
  excluirMateria(id: number): Observable<void> {
    if (!this.isBrowser) return new Observable<void>();
    
    console.log(`Excluindo matéria ${id}`);
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(() => {
        console.log(`Matéria ${id} excluída com sucesso`);
        
        // Remover do array local
        const materiasAtuais = this.materiasSubject.value;
        this.materiasSubject.next(materiasAtuais.filter(m => m.id !== id));
        
        // Remover da lista filtrada
        const materiasFiltradas = this.materiasFiltradosSubject.value;
        this.materiasFiltradosSubject.next(materiasFiltradas.filter(m => m.id !== id));
      }),
      catchError(this.handleError<void>(`excluirMateria(${id})`))
    );
  }
}