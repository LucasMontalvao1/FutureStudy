import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { User } from '../models/user';
import { LoginRequest } from '../models/login-request.model';
import { JwtHelperService } from '@auth0/angular-jwt';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = environment.endpoints.login;
  private jwtHelper = new JwtHelperService();
  private platformId = inject(PLATFORM_ID);

  constructor(private http: HttpClient) { }

  // Método para realizar o login
  login(loginRequest: LoginRequest): Observable<User> {
    return this.http.post<User>(this.apiUrl, loginRequest).pipe(
      catchError(this.handleError) // Captura erros da API
    );
  }

  // Verifica se estamos rodando no navegador
  private isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  // Armazena o token no sessionStorage (apenas no navegador)
  storeToken(token: string): void {
    if (this.isBrowser()) {
      sessionStorage.setItem('token', token);
    }
  }

  // Retorna o token armazenado (apenas no navegador)
  getToken(): string | null {
    if (this.isBrowser()) {
      const token = sessionStorage.getItem('token');
      return token;
    }
    return null;
  }

  // Verifica se o token ainda é válido (seguro para SSR)
  isAuthenticated(): boolean {
    if (!this.isBrowser()) {
      return false; // No servidor, considere não autenticado
    }
    const token = this.getToken();
    return token ? !this.jwtHelper.isTokenExpired(token) : false;
  }

  // Decodifica e retorna os dados do token (seguro para SSR)
  getDecodedToken(): any {
    if (!this.isBrowser()) {
      return null; // No servidor, retorne null
    }
    const token = this.getToken();
    const decodedToken = token ? this.jwtHelper.decodeToken(token) : null;
    return decodedToken;
  }

  // Novo método para obter o ID do usuário do token (seguro para SSR)
  getCurrentUserId(): number {
    if (!this.isBrowser()) {
      return 0; // No servidor, retorne 0
    }
    const decodedToken = this.getDecodedToken();
    return decodedToken ? decodedToken.usuarioID : 0; 
  }

  // Limpa o token do sessionStorage, efetua logout (seguro para SSR)
  logout(): void {
    if (this.isBrowser()) {
      sessionStorage.removeItem('token');
    }
  }

  // Método para tratar erros da API
  private handleError(error: any): Observable<never> {
    let errorMessage = 'Ocorreu um erro desconhecido!';
    switch (error.status) {
      case 401:
        errorMessage = 'Não autorizado! Verifique suas credenciais.';
        break;
      default:
        if (error.error instanceof ErrorEvent) {
          // Erro no lado do cliente
          errorMessage = `Erro: ${error.error.message}`;
        } else {
          // Erro no lado do servidor
          errorMessage = `Código do Erro: ${error.status}\nMensagem: ${error.message}`;
        }
    }
    console.error('Erro capturado:', errorMessage);
    return throwError(errorMessage);
  }
}