import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { MainLayoutComponent } from './main-layout.component';
import { AuthService } from '../services/auth.service';

describe('MainLayoutComponent', () => {
  let component: MainLayoutComponent;
  let fixture: ComponentFixture<MainLayoutComponent>;
  let authService: AuthService;

  beforeEach(async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      declarations: [MainLayoutComponent],
      imports: [RouterTestingModule],
      providers: [
        AuthService,
        { provide: Router, useValue: { navigate: vi.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(MainLayoutComponent);
    component = fixture.componentInstance;
    authService = TestBed.inject(AuthService);
    localStorage.clear();
  });

  it('returns user name from localStorage', () => {
    localStorage.setItem('auth_user', JSON.stringify({ firstName: 'John', lastName: 'Doe' }));
    expect(component.userName).toBe('John');
  });

  it('returns empty string when no user data', () => {
    expect(component.userName).toBe('');
  });

  it('isRole checks against stored role', () => {
    localStorage.setItem('auth_role', 'Admin');
    expect(component.isRole('Admin')).toBe(true);
    expect(component.isRole('Employee')).toBe(false);
  });

  it('logout clears session and navigates', () => {
    const router = TestBed.inject(Router);
    localStorage.setItem('auth_token', 'token');
    component.logout();
    expect(localStorage.getItem('auth_token')).toBeNull();
    expect(router.navigate).toHaveBeenCalledWith(['/login']);
  });
});
