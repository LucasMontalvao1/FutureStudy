import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class MetasService {
  private apiUrl = environment.endpoints.metas;
  

  constructor(
      private http: HttpClient,
      private authService: AuthService
    ) { }

   // Método para obter headers com o token
    private getHeaders(): HttpHeaders {
      let headers = new HttpHeaders({
        'Content-Type': 'application/json'
      });
  
      const token = this.authService.getToken();
      if (token) {
        headers = headers.set('Authorization', `Bearer ${token}`);
      }
  
      return headers;
    }
  

  // Método para visualizar o token atual (apenas para debug)
  private logTokenInfo() {
    const token = this.authService.getToken();
    console.log('Token disponível?', !!token);
    if (token) {
      console.log('Token (primeiros 30 caracteres):', token.substring(0, 30) + '...');
      
      // Decodificar e mostrar o payload do token JWT
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log('Token payload:', payload);
        
        // Verificar expiração
        if (payload.exp) {
          const expDate = new Date(payload.exp * 1000);
          const now = new Date();
          console.log('Token expira em:', expDate.toLocaleString());
          console.log('Token expirado?', expDate < now);
        }
      } catch (e) {
        console.error('Erro ao decodificar token:', e);
      }
    }
  }

  // Obter todas as metas do usuário
  getAllMetas(): Observable<any[]> {
    console.log('=== Iniciando getAllMetas ===');
    
    return this.http.get<any[]>(this.apiUrl, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log('Resposta getAllMetas:', response);
        console.log('=== Requisição getAllMetas bem-sucedida ===');
      }),
      catchError(error => {
        console.error('Erro ao buscar todas as metas:', error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of([]);
      })
    );
  }

  // Obter metas por data
  getMetasByData(data: string): Observable<any[]> {
    console.log('=== Iniciando getMetasByData ===');
    
    // Parâmetros conforme esperado pela API
    const params = new HttpParams()
      .append('dataInicio', data)
      .append('dataFim', data);
    
    // Log da URL completa para depuração
    console.log(`URL da requisição: ${this.apiUrl}/por-data com parâmetros:`, params.toString());
    
    return this.http.get<any[]>(`${this.apiUrl}/por-data`, { 
      headers: this.getHeaders(),
      params 
    }).pipe(
      tap(response => {
        console.log(`Resposta getMetasByData para ${data}:`, response);
        console.log('=== Requisição getMetasByData bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Falha ao buscar metas para ${data}. Erro:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of([]);
      })
    );
  }

  // Obter metas ativas do usuário
  getMetasAtivas(): Observable<any[]> {
    console.log('=== Iniciando getMetasAtivas ===');
    
    return this.http.get<any[]>(`${this.apiUrl}/ativas`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log('Resposta getMetasAtivas:', response);
        console.log('=== Requisição getMetasAtivas bem-sucedida ===');
      }),
      catchError(error => {
        console.error('Erro ao buscar metas ativas:', error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of([]);
      })
    );
  }

  // Obter metas por matéria
  getMetasByMateria(materiaId: number): Observable<any[]> {
    console.log(`=== Iniciando getMetasByMateria para matéria ${materiaId} ===`);
    
    return this.http.get<any[]>(`${this.apiUrl}/materia/${materiaId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Resposta getMetasByMateria para ID ${materiaId}:`, response);
        console.log('=== Requisição getMetasByMateria bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao buscar metas para matéria ${materiaId}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of([]);
      })
    );
  }

  // Obter metas por tópico
  getMetasByTopico(topicoId: number): Observable<any[]> {
    console.log(`=== Iniciando getMetasByTopico para tópico ${topicoId} ===`);
    
    return this.http.get<any[]>(`${this.apiUrl}/topico/${topicoId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Resposta getMetasByTopico para ID ${topicoId}:`, response);
        console.log('=== Requisição getMetasByTopico bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao buscar metas para tópico ${topicoId}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of([]);
      })
    );
  }

  // Obter meta por ID
  getMetaById(id: number): Observable<any> {
    console.log(`=== Iniciando getMetaById para meta ${id} ===`);
    
    return this.http.get<any>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Resposta getMetaById para ID ${id}:`, response);
        console.log('=== Requisição getMetaById bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao buscar meta ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of(null);
      })
    );
  }

  // Criar uma nova meta
  createMeta(meta: any): Observable<any> {
    console.log('=== Iniciando createMeta ===');
    console.log('Dados da meta a criar:', meta);
    
    return this.http.post<any>(this.apiUrl, meta, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log('Meta criada com sucesso:', response);
        console.log('=== Requisição createMeta bem-sucedida ===');
      }),
      catchError(error => {
        console.error('Erro ao criar meta:', error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }

  // Atualizar uma meta existente
  updateMeta(id: number, meta: any): Observable<any> {
    console.log(`=== Iniciando updateMeta para meta ${id} ===`);
    console.log('Dados da meta a atualizar:', meta);
    
    return this.http.put<any>(`${this.apiUrl}/${id}`, meta, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Meta ${id} atualizada com sucesso:`, response);
        console.log('=== Requisição updateMeta bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao atualizar meta ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }

  // Atualizar o progresso de uma meta
  updateProgressoMeta(id: number, quantidade: number): Observable<any> {
    console.log(`=== Iniciando updateProgressoMeta para meta ${id} ===`);
    console.log(`Quantidade a atualizar: ${quantidade}`);
    
    return this.http.patch<any>(`${this.apiUrl}/${id}/progresso`, quantidade, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Progresso da meta ${id} atualizado:`, response);
        console.log('=== Requisição updateProgressoMeta bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao atualizar progresso da meta ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }

  // Marcar uma meta como concluída
  concluirMeta(id: number): Observable<any> {
    console.log(`=== Iniciando concluirMeta para meta ${id} ===`);
    
    return this.http.patch<any>(`${this.apiUrl}/${id}/concluir`, {}, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Meta ${id} concluída:`, response);
        console.log('=== Requisição concluirMeta bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao concluir meta ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }

  // Excluir uma meta
  deleteMeta(id: number): Observable<any> {
    console.log(`=== Iniciando deleteMeta para meta ${id} ===`);
    
    return this.http.delete<any>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Meta ${id} excluída:`, response);
        console.log('=== Requisição deleteMeta bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao excluir meta ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }
}