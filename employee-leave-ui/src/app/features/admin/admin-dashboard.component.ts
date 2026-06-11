import { Component, OnInit } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { UserResponse } from '../../core/models/user.model';
import { LeaveRequestResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-admin-dashboard',
  standalone: false,
  templateUrl: './admin-dashboard.component.html'
})
export class AdminDashboardComponent implements OnInit {
  users: UserResponse[] = [];
  allRequests: LeaveRequestResponse[] = [];
  loading = true;

  constructor(
    private userService: UserService,
    private leaveRequestService: LeaveRequestService
  ) {}

  ngOnInit(): void {
    this.userService.getAll().subscribe(users => {
      this.users = users;
    });

    this.leaveRequestService.getAll().subscribe(requests => {
      this.allRequests = requests;
      this.loading = false;
    });
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
