import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class AnotacoesService {
  private apiUrl = environment.endpoints.anotacoes;

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

  // Método para visualizar o token atual
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

  // Obter o próximo dia
  private getNextDay(dateStr: string): string {
    // Parse a data MM/dd/yyyy
    const parts = dateStr.split('/');
    const date = new Date(
      parseInt(parts[2]), // ano
      parseInt(parts[0]) - 1, // mês (0-11)
      parseInt(parts[1]) // dia
    );
    
    // Adiciona 1 dia
    date.setDate(date.getDate() + 1);
    
    // Formata de volta para MM/dd/yyyy
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const year = date.getFullYear();
    
    return `${month}/${day}/${year}`;
  }

  // Obter anotações por data
  getAnotacoesByData(data: string): Observable<any[]> {

    // Log do token antes da requisição
    console.log('=== Iniciando getAnotacoesByData ===');
    this.logTokenInfo();
    
    // Parâmetros conforme esperado pela API
    const dataFim = this.getNextDay(data);
    const params = new HttpParams()
      .append('dataInicio', data)
      .append('dataFim', dataFim);
    
    // Log da URL completa para depuração
    console.log(`URL da requisição: ${this.apiUrl}/por-data com parâmetros:`, params.toString());
    console.log(`Buscando anotações de ${data} até ${dataFim}`);
    
    return this.http.get<any[]>(`${this.apiUrl}/por-data`, { 
      headers: this.getHeaders(),
      params 
    }).pipe(
      tap(response => {
        console.log(`Resposta getAnotacoesByData para ${data}:`, response);
        console.log('=== Requisição getAnotacoesByData bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Falha ao buscar anotações para ${data}. Erro:`, error);
        console.error('Detalhes do erro:', {
          status: error.status,
          statusText: error.statusText,
          url: error.url,
          message: error.message,
          error: error.error
        });
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        console.log('=== Fim da requisição getAnotacoesByData com erro ===');
        return of([]);
      })
    );
  }
  
  // Obter anotações por sessão de estudo
  getAnotacoesBySessao(sessaoId: number): Observable<any[]> {

    console.log('=== Iniciando getAnotacoesBySessao ===');
    this.logTokenInfo();
    
    return this.http.get<any[]>(`${this.apiUrl}/sessao/${sessaoId}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Resposta getAnotacoesBySessao para ID ${sessaoId}:`, response);
        console.log('=== Requisição getAnotacoesBySessao bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao buscar anotações para sessão ${sessaoId}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of([]);
      })
    );
  }

  // Obter anotação por ID
  getAnotacaoById(id: number): Observable<any> {

    console.log('=== Iniciando getAnotacaoById ===');
    this.logTokenInfo();
    
    return this.http.get<any>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Resposta getAnotacaoById para ID ${id}:`, response);
        console.log('=== Requisição getAnotacaoById bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao buscar anotação ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        return of(null);
      })
    );
  }

  // Criar uma nova anotação
  createAnotacao(anotacao: any): Observable<any> {

    console.log('=== Iniciando createAnotacao ===');
    this.logTokenInfo();
    
    return this.http.post<any>(this.apiUrl, anotacao, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log('Anotação criada com sucesso:', response);
        console.log('=== Requisição createAnotacao bem-sucedida ===');
      }),
      catchError(error => {
        console.error('Erro ao criar anotação:', error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }

  // Atualizar uma anotação existente
  updateAnotacao(id: number, anotacao: any): Observable<any> {

    console.log('=== Iniciando updateAnotacao ===');
    this.logTokenInfo();
    
    return this.http.put<any>(`${this.apiUrl}/${id}`, anotacao, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Anotação ${id} atualizada com sucesso:`, response);
        console.log('=== Requisição updateAnotacao bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao atualizar anotação ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }

  // Excluir uma anotação
  deleteAnotacao(id: number): Observable<any> {

    console.log('=== Iniciando deleteAnotacao ===');
    this.logTokenInfo();
    
    return this.http.delete<any>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    }).pipe(
      tap(response => {
        console.log(`Anotação ${id} excluída:`, response);
        console.log('=== Requisição deleteAnotacao bem-sucedida ===');
      }),
      catchError(error => {
        console.error(`Erro ao excluir anotação ${id}:`, error);
        
        if (error.status === 401) {
          console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
          this.logTokenInfo();
        }
        
        throw error;
      })
    );
  }
}