import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class SessoesEstudoService {
  private apiUrl = environment.endpoints.sessaoestudo;

  constructor(private http: HttpClient) { }

  // Método privado para obter headers com o token
  private getHeaders(): HttpHeaders {
    const token = localStorage.getItem('token');
    return new HttpHeaders({
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    });
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
    let params = new HttpParams().append('periodo', periodo);
    
    if (data) {
      params = params.append('data', data.toISOString());
    }

    return this.http.get<any>(`${this.apiUrl}/dashboard`, {
      headers: this.getHeaders(),
      params
    });
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