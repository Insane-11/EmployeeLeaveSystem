import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-main-layout',
  standalone: false,
  templateUrl: './main-layout.component.html'
})
export class MainLayoutComponent {
  collapsed = true;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  get userName(): string {
    const stored = localStorage.getItem('auth_user');
    if (stored) {
      try {
        const user = JSON.parse(stored);
        return user.firstName || user.email;
      } catch {
        return '';
      }
    }
    return '';
  }

  isRole(role: string): boolean {
    return this.authService.getUserRole() === role;
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  toggleSidebar(): void {
    this.collapsed = !this.collapsed;
  }
}
