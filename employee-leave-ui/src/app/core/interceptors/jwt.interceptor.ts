import { Injectable } from '@angular/core';
import {
  HttpInterceptor, HttpRequest, HttpHandler, HttpEvent,
  HttpErrorResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, timeout } from 'rxjs/operators';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { LoadingService } from '../services/loading.service';

@Injectable()
export class JwtInterceptor implements HttpInterceptor {
  private skipPaths = ['/api/auth/login', '/api/auth/register'];

  constructor(
    private authService: AuthService,
    private toastService: ToastService,
    private loadingService: LoadingService,
    private router: Router
  ) {}

  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    if (!this.skipPaths.some(p => req.url.includes(p))) {
      this.loadingService.show();
    }

    const token = this.authService.getToken();
    if (token) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }

    return next.handle(req).pipe(
      timeout(15000),
      finalize(() => this.loadingService.hide()),
      catchError((error: any) => {
        if (error.name === 'TimeoutError') {
          this.toastService.error('Request timed out. Please try again.');
        } else if (error instanceof HttpErrorResponse) {
          if (error.status === 401) {
            this.authService.logout();
            this.router.navigate(['/login']);
            this.toastService.error('Session expired. Please login again.');
          } else if (error.status === 403) {
            this.toastService.error('You do not have permission to perform this action.');
          } else if (error.status === 404) {
            this.toastService.error('Resource not found.');
          } else if (error.status >= 400 && error.status < 500) {
            const msg = error.error?.message || error.error?.title || 'Invalid request.';
            this.toastService.error(msg);
          } else if (error.status >= 500) {
            this.toastService.error('Server error. Please try again later.');
          }
        }
        return throwError(() => error);
      })
    );
  }
}
