import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { UserResponse, UpdateUserRequest } from '../../core/models/user.model';

@Component({
  selector: 'app-user-management',
  standalone: false,
  templateUrl: './user-management.component.html'
})
export class UserManagementComponent implements OnInit, OnDestroy {
  users: UserResponse[] = [];
  loading = true;
  editUser: UserResponse | null = null;
  editModel: UpdateUserRequest = {};
  private timeoutRef: any;

  constructor(
    private userService: UserService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.loadUsers();
  }

  ngOnDestroy(): void {
    clearTimeout(this.timeoutRef);
  }

  loadUsers(): void {
    this.loading = true;
    this.timeoutRef = setTimeout(() => this.loading = false, 5000);
    this.userService.getAll().subscribe({
      next: users => {
        this.users = users;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => this.loading = false
    });
  }

  openEdit(user: UserResponse): void {
    this.editUser = user;
    const names = user.fullName.split(' ');
    this.editModel = {
      firstName: names[0],
      lastName: names.slice(1).join(' '),
      department: user.department || '',
      isActive: user.isActive
    };
  }

  saveEdit(): void {
    if (!this.editUser) return;
    this.userService.update(this.editUser.id, this.editModel).subscribe({
      next: () => {
        this.editUser = null;
        this.loadUsers();
      },
      error: () => alert('Failed to update user.')
    });
  }

  confirmDelete(user: UserResponse): void {
    if (confirm(`Delete user "${user.fullName}"? This cannot be undone.`)) {
      this.userService.delete(user.id).subscribe({
        next: () => this.loadUsers(),
        error: () => alert('Failed to delete user.')
      });
    }
  }
}
