import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RoleGuard } from './role.guard';
import { AuthService } from '../services/auth.service';

describe('RoleGuard', () => {
  let guard: RoleGuard;
  let authService: AuthService;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        RoleGuard,
        AuthService,
        { provide: Router, useValue: { navigate: vi.fn(), parseUrl: vi.fn().mockReturnValue({ toString: () => '/' }) } }
      ]
    });

    guard = TestBed.inject(RoleGuard);
    authService = TestBed.inject(AuthService);
  });

  it('returns true when user has required role', () => {
    vi.spyOn(authService, 'getUserRole').mockReturnValue('Admin');
    const route: any = { data: { roles: ['Admin'] } };
    expect(guard.canActivate(route)).toBe(true);
  });

  it('redirects when user lacks required role', () => {
    vi.spyOn(authService, 'getUserRole').mockReturnValue('Employee');
    const route: any = { data: { roles: ['Admin'] } };
    const result = guard.canActivate(route);
    expect(result.toString()).toContain('/');
  });
});
