import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { catchError, Observable, of, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { AuthService } from './auth.service';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class SessoesEstudoService {
  private apiUrl = environment.endpoints.sessaoestudo;
  isBrowser: any;

  constructor(
      private http: HttpClient,
      private authService: AuthService,
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

  // Obter sessões por período
  getSessoesByPeriodo(inicio?: Date, fim?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (inicio) {
      params = params.append('inicio', inicio.toISOString());
    }
    if (fim) {
      params = params.append('fim', fim.toISOString());
    }

    return this.http.get<any[]>(this.apiUrl, {
      headers: this.getHeaders(),
      params
    });
  }

  // Obter sessão por ID
  getSessaoById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`, {
      headers: this.getHeaders()
    });
  }

  // Obter dados do calendário
  getCalendario(mes: number, ano: number): Observable<any[]> {
    const params = new HttpParams()
      .append('mes', mes.toString())
      .append('ano', ano.toString());

    return this.http.get<any[]>(`${this.apiUrl}/calendario`, {
      headers: this.getHeaders(),
      params
    });
  }

  // Obter estatísticas do dashboard
getDashboard(periodo: string = 'semana', data?: Date): Observable<any> {
  // Verificar se estamos no navegador antes de prosseguir
  if (!this.isBrowser) {
    return of(null);
  }
  
  let params = new HttpParams().set('periodo', periodo);

  // Adiciona o parâmetro 'data' no formato MM/dd/yyyy
  if (data) {
    const dia = String(data.getDate()).padStart(2, '0');
    const mes = String(data.getMonth() + 1).padStart(2, '0');
    const ano = data.getFullYear();
    const dataFormatada = `${mes}/${dia}/${ano}`;
    params = params.set('data', dataFormatada);
  }

  const token = this.authService.getToken();
  console.log('Token existe?', !!token);
  console.log(token);
  
  // Só prosseguir com a requisição se tiver token
  if (!token) {
    console.log('Token não encontrado, não fazendo requisição');
    return of(null);
  }
  
  return this.http.get<any>(`${this.apiUrl}/dashboard`, {
    headers: this.getHeaders(),
    params
  }).pipe(
    tap(response => {
      console.log('Dashboard recebido:', response);
    }),
    catchError(error => {
      console.error('Erro ao obter dashboard:', error);
      return of(null);
    })
  );
}

  // Iniciar uma nova sessão de estudo
  iniciarSessao(sessao: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, sessao, {
      headers: this.getHeaders()
    });
  }

  // Pausar uma sessão em andamento
  pausarSessao(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/pausar`, {}, {
      headers: this.getHeaders()
    });
  }

  // Retomar uma sessão pausada
  retomarSessao(pausaId: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/pausas/${pausaId}/retomar`, {}, {
      headers: this.getHeaders()
    });
  }

  // Finalizar uma sessão
  finalizarSessao(id: number): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/finalizar`, {}, {
      headers: this.getHeaders()
    });
  }
}