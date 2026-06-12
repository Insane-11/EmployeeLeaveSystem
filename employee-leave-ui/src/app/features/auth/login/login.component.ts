import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../core/models/auth.model';

@Component({
  selector: 'app-login',
  standalone: false,
  templateUrl: './login.component.html'
})
export class LoginComponent {
  model: LoginRequest = { email: '', password: '' };
  error = '';

  constructor(private authService: AuthService, private router: Router) {}

  quickLogin(role: string): void {
    this.error = '';
    if (role === 'admin') {
      this.model = { email: 'admin@admin.com', password: '' };
    } else {
      this.model = { email: '', password: '' };
    }
  }

  onSubmit(): void {
    this.error = '';
    this.authService.login(this.model).subscribe({
      next: () => {
        const role = this.authService.getUserRole();
        if (role === 'Admin') {
          this.router.navigate(['/admin']);
        } else if (role === 'Manager') {
          this.router.navigate(['/manager']);
        } else {
          this.router.navigate(['/employee']);
        }
      },
      error: () => {
        this.error = 'Invalid email or password';
      }
    });
  }
}
