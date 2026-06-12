import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { UserResponse } from '../../core/models/user.model';
import { LeaveRequestResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit, OnDestroy {
  users: UserResponse[] = [];
  allRequests: LeaveRequestResponse[] = [];
  loading = true;
  private timeoutRef: any;

  constructor(
    private userService: UserService,
    private leaveRequestService: LeaveRequestService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.timeoutRef = setTimeout(() => this.loading = false, 5000);
    this.userService.getAll().subscribe({
      next: users => this.users = users,
      error: () => this.loading = false
    });

    this.leaveRequestService.getAll().subscribe({
      next: requests => {
        this.allRequests = requests;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => this.loading = false
    });
  }

  ngOnDestroy(): void {
    clearTimeout(this.timeoutRef);
  }

  get totalUsers(): number {
    return this.users.length;
  }

  get totalRequestsThisYear(): number {
    const year = new Date().getFullYear();
    return this.allRequests.filter(r => new Date(r.submittedAt).getFullYear() === year).length;
  }

  get pendingApprovals(): number {
    return this.allRequests.filter(r => r.status === 'Pending').length;
  }
}
