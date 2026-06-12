import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authService: AuthService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [FormsModule],
      providers: [
        AuthService,
        ToastService,
        { provide: Router, useValue: { navigate: vi.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService);
    localStorage.clear();
    fixture.detectChanges();
  });

  it('renders email and password fields', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    expect(compiled.querySelector('input[type="email"]')).toBeTruthy();
    expect(compiled.querySelector('input[type="password"]')).toBeTruthy();
  });

  it('calls authService.login on valid form', () => {
    const loginSpy = vi.spyOn(authService, 'login').mockReturnValue(of({
      token: 'abc', email: 'test@test.com', role: 'Employee', firstName: 'T', lastName: 'U'
    }));
    component.model = { email: 'test@test.com', password: 'pass123' };
    component.onSubmit();
    expect(loginSpy).toHaveBeenCalledWith({ email: 'test@test.com', password: 'pass123' });
  });

  it('navigates to /employee for Employee role', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(authService, 'login').mockReturnValue(of({
      token: 'abc', email: 'test@test.com', role: 'Employee', firstName: 'T', lastName: 'U'
    }));
    component.onSubmit();
    expect(router.navigate).toHaveBeenCalledWith(['/employee']);
  });

  it('navigates to /admin for Admin role', () => {
    const router = TestBed.inject(Router);
    vi.spyOn(authService, 'login').mockReturnValue(of({
      token: 'abc', email: 'admin@test.com', role: 'Admin', firstName: 'A', lastName: 'U'
    }));
    vi.spyOn(authService, 'getUserRole').mockReturnValue('Admin');
    component.onSubmit();
    expect(router.navigate).toHaveBeenCalledWith(['/admin']);
  });

  it('shows error on login failure', () => {
    vi.spyOn(authService, 'login').mockReturnValue(throwError(() => new Error('Invalid credentials')));
    component.onSubmit();
    expect(component.error).toBe('Invalid email or password');
  });
});
