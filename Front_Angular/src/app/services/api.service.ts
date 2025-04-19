import { Injectable, PLATFORM_ID, Inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class ApiService {
  protected isBrowser: boolean;

  constructor(
    protected http: HttpClient,
    protected authService: AuthService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  // Método para obter headers com o token
  protected getHeaders(): HttpHeaders {
    let headers = new HttpHeaders({
      'Content-Type': 'application/json'
    });

    if (this.isBrowser) {
      const token = this.authService.getToken();
      if (token) {
        headers = headers.set('Authorization', `Bearer ${token}`);
      }
    }

    return headers;
  }

  // Método genérico para tratamento de erros
  protected handleError<T>(operation = 'operation', result?: T) {
    return (error: any): Observable<T> => {
      console.error(`Erro na operação ${operation}:`, error);
      
      if (error.status === 401) {
        console.error('ERRO DE AUTENTICAÇÃO: Token inválido ou expirado');
      }
      
      if (error?.error?.cause?.code === 'DEPTH_ZERO_SELF_SIGNED_CERT') {
        console.warn('Erro de certificado SSL. Verifique a configuração do ambiente de desenvolvimento.');
      }
      
      return of(result as T);
    };
  }
}
