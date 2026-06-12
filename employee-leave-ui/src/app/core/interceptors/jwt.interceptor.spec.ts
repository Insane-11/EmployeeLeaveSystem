import { TestBed } from '@angular/core/testing';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { LoadingService } from '../services/loading.service';
import { JwtInterceptor } from './jwt.interceptor';

describe('JwtInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: AuthService;

  beforeEach(() => {
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        AuthService,
        ToastService,
        LoadingService,
        { provide: Router, useValue: { navigate: vi.fn() } },
        { provide: HTTP_INTERCEPTORS, useClass: JwtInterceptor, multi: true }
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('attaches Bearer token from localStorage', () => {
    localStorage.setItem('auth_token', 'test-token-123');
    http.get('/api/test').subscribe();
    const req = httpMock.expectOne('/api/test');
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token-123');
    req.flush({});
  });

  it('skips auth header for /api/auth/login', () => {
    localStorage.setItem('auth_token', 'secret');
    http.post('/api/auth/login', {}).subscribe();
    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.headers.get('Authorization')).toBeNull();
    req.flush({});
  });

  it('skips auth header for /api/auth/register', () => {
    localStorage.setItem('auth_token', 'secret');
    http.post('/api/auth/register', {}).subscribe();
    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.headers.get('Authorization')).toBeNull();
    req.flush({});
  });

  it('shows loading for non-auth requests', () => {
    const loadingService = TestBed.inject(LoadingService);
    http.get('/api/test').subscribe();
    expect(loadingService.loading).toBe(true);
    const req = httpMock.expectOne('/api/test');
    req.flush({});
  });

  it('does not show loading for auth requests', () => {
    const loadingService = TestBed.inject(LoadingService);
    http.post('/api/auth/login', {}).subscribe();
    expect(loadingService.loading).toBe(false);
    const req = httpMock.expectOne('/api/auth/login');
    req.flush({});
  });
});
