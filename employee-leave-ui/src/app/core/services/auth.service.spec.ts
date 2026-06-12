import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    localStorage.clear();
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('login POSTs to /api/auth/login', () => {
    service.login({ email: 'test@test.com', password: 'pass123' }).subscribe();
    const req = httpMock.expectOne('/api/auth/login');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ email: 'test@test.com', password: 'pass123' });
    req.flush({ token: 'abc', email: 'test@test.com', role: 'Employee', firstName: 'Test', lastName: 'User' });
  });

  it('login stores token in localStorage', () => {
    service.login({ email: 'test@test.com', password: 'pass123' }).subscribe();
    const req = httpMock.expectOne('/api/auth/login');
    req.flush({ token: 'abc123', email: 'test@test.com', role: 'Employee', firstName: 'Test', lastName: 'User' });
    expect(localStorage.getItem('auth_token')).toBe('abc123');
    expect(localStorage.getItem('auth_role')).toBe('Employee');
  });

  it('register POSTs to /api/auth/register', () => {
    const reg = { firstName: 'New', lastName: 'User', email: 'new@test.com', password: 'pass123' };
    service.register(reg).subscribe();
    const req = httpMock.expectOne('/api/auth/register');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reg);
    req.flush({ message: 'Registration successful' });
  });

  it('getToken returns token from localStorage', () => {
    localStorage.setItem('auth_token', 'my-token');
    expect(service.getToken()).toBe('my-token');
  });

  it('getToken returns null when no token', () => {
    expect(service.getToken()).toBeNull();
  });

  it('isLoggedIn returns true when token exists', () => {
    localStorage.setItem('auth_token', 'token');
    expect(service.isLoggedIn()).toBe(true);
  });

  it('isLoggedIn returns false when no token', () => {
    expect(service.isLoggedIn()).toBe(false);
  });

  it('logout clears localStorage', () => {
    localStorage.setItem('auth_token', 'token');
    localStorage.setItem('auth_role', 'Admin');
    service.logout();
    expect(localStorage.getItem('auth_token')).toBeNull();
    expect(localStorage.getItem('auth_role')).toBeNull();
  });

  it('getUserRole returns role from localStorage', () => {
    localStorage.setItem('auth_role', 'Manager');
    expect(service.getUserRole()).toBe('Manager');
  });
});
