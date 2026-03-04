import { Injectable, inject } from '@angular/core';
import {
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, throwError, switchMap, catchError } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  private authService = inject(AuthService);

  intercept(
    req: HttpRequest<any>,
    next: HttpHandler,
  ): Observable<HttpEvent<any>> {
    const isAuthUrl = req.url.includes('/api/Auth/');

    const token = this.authService.getAccessToken();

    if (token && !isAuthUrl) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`,
        },
      });
    }

    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !isAuthUrl) {
          return this.authService.refreshToken()!.pipe(
            switchMap((newUser) => {
              const newReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${newUser.accessToken}`,
                },
              });
              return next.handle(newReq);
            }),
            catchError((err) => {
              this.authService.logout();
              return throwError(() => err);
            }),
          );
        }

        return throwError(() => error);
      }),
    );
  }
}
