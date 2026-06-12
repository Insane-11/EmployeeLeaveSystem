import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { UserResponse } from '../../core/models/user.model';
import { LeaveRequestResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-manager-dashboard',
  standalone: false,
  templateUrl: './manager-dashboard.component.html'
})
export class ManagerDashboardComponent implements OnInit, OnDestroy {
  teamMembers: UserResponse[] = [];
  teamRequests: LeaveRequestResponse[] = [];
  loading = true;
  private timeoutRef: any;

  constructor(
    private userService: UserService,
    private leaveRequestService: LeaveRequestService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.timeoutRef = setTimeout(() => this.loading = false, 5000);
    this.userService.getTeam().subscribe({
      next: members => this.teamMembers = members,
      error: () => this.loading = false
    });

    this.leaveRequestService.getTeamRequests().subscribe({
      next: requests => {
        this.teamRequests = requests;
        this.loading = false;
        this.cdr.detectChanges();
      },
      error: () => this.loading = false
    });
  }

  ngOnDestroy(): void {
    clearTimeout(this.timeoutRef);
  }

  get pendingCount(): number {
    return this.teamRequests.filter(r => r.status === 'Pending').length;
  }
}
