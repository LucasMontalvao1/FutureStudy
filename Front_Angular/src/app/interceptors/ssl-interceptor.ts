import { HttpInterceptorFn } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { inject } from '@angular/core';

// Definindo o Interceptor
export const sslInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService); // Injetando o AuthService

  // Clone a requisição e definir a propriedade withCredentials como false para lidar com certificados
  const modifiedReq = req.clone({
    withCredentials: false,
    setHeaders: {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      // Usando o AuthService para pegar o token
      'Authorization': `Bearer ${authService.getToken()}`
    }
  });

  return next(modifiedReq);
};
