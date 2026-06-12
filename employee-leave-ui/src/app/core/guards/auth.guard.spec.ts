import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { AuthGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('AuthGuard', () => {
  let guard: AuthGuard;
  let authService: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        AuthGuard,
        AuthService,
        { provide: Router, useValue: { navigate: vi.fn(), createUrlTree: vi.fn().mockReturnValue({ toString: () => '/login' }), parseUrl: vi.fn().mockReturnValue({ toString: () => '/login' }) } }
      ]
    });

    guard = TestBed.inject(AuthGuard);
    authService = TestBed.inject(AuthService);
  });

  it('returns true when user is logged in', () => {
    vi.spyOn(authService, 'isLoggedIn').mockReturnValue(true);
    expect(guard.canActivate()).toBe(true);
  });

  it('redirects to /login when not logged in', () => {
    vi.spyOn(authService, 'isLoggedIn').mockReturnValue(false);
    const result = guard.canActivate();
    expect(result.toString()).toContain('/login');
  });
});
