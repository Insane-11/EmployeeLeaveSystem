import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { RegisterComponent } from './register.component';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authService: AuthService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      imports: [FormsModule],
      providers: [
        AuthService,
        ToastService,
        { provide: Router, useValue: { navigate: vi.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService);
    fixture.detectChanges();
  });

  it('renders all form fields', () => {
    const el = fixture.nativeElement as HTMLElement;
    expect(el.querySelector('input#firstName')).toBeTruthy();
    expect(el.querySelector('input#lastName')).toBeTruthy();
    expect(el.querySelector('input#email')).toBeTruthy();
    expect(el.querySelector('input#password')).toBeTruthy();
    expect(el.querySelector('input#confirmPassword')).toBeTruthy();
  });

  it('calls authService.register on submit', () => {
    const spy = vi.spyOn(authService, 'register').mockReturnValue(of({ message: 'OK' }));
    component.model = {
      firstName: 'New', lastName: 'User', email: 'new@test.com',
      password: 'Pass123!', confirmPassword: 'Pass123!', roleId: 3
    };
    component.onSubmit();
    expect(spy).toHaveBeenCalledWith({
      firstName: 'New', lastName: 'User', email: 'new@test.com',
      password: 'Pass123!', confirmPassword: 'Pass123!', roleId: 3
    });
  });

  it('sets success to true on successful registration', () => {
    vi.spyOn(authService, 'register').mockReturnValue(of({ message: 'OK' }));
    component.onSubmit();
    expect(component.success).toBe(true);
  });

  it('shows error on registration failure', () => {
    vi.spyOn(authService, 'register').mockReturnValue(throwError(() => new Error('Email exists')));
    component.onSubmit();
    expect(component.error).toBe('Registration failed. Email may already be in use.');
  });
});
