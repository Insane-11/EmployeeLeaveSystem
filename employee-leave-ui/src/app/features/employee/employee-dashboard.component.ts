import { Component, OnInit, OnDestroy } from '@angular/core';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { LeaveRequestResponse, LeaveBalanceResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-employee-dashboard',
  standalone: false,
  templateUrl: './employee-dashboard.component.html'
})
export class EmployeeDashboardComponent implements OnInit, OnDestroy {
  balances: LeaveBalanceResponse[] = [];
  recentRequests: LeaveRequestResponse[] = [];
  loading = true;
  private timeoutRef: any;

  constructor(private leaveRequestService: LeaveRequestService) {}

  ngOnInit(): void {
    this.timeoutRef = setTimeout(() => this.loading = false, 5000);
    this.leaveRequestService.getMyBalance().subscribe({
      next: balances => this.balances = balances.filter(b => b.remainingDays > 0 || ['Annual', 'Sick', 'Personal'].includes(b.leaveType)),
      error: () => this.loading = false
    });

    this.leaveRequestService.getMyRequests().subscribe({
      next: requests => { this.recentRequests = requests.slice(0, 5); this.loading = false; },
      error: () => this.loading = false
    });
  }

  ngOnDestroy(): void {
    clearTimeout(this.timeoutRef);
  }
}
