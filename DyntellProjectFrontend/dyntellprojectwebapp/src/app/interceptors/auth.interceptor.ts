import { HttpInterceptorFn } from '@angular/common/http';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Token lekeresese a localStorage-bol
  const token = localStorage.getItem('authToken');
  
  // Ha van token, hozzaadjuk az Authorization header-hez
  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(clonedRequest);
  }
  
  // Ha nincs token, tovabbadjuk valtozatlanul
  return next(req);
};

