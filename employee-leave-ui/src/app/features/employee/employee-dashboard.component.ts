import { Component, OnInit } from '@angular/core';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { LeaveRequestResponse, LeaveBalanceResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-employee-dashboard',
  standalone: false,
  templateUrl: './employee-dashboard.component.html'
})
export class EmployeeDashboardComponent implements OnInit {
  balances: LeaveBalanceResponse[] = [];
  recentRequests: LeaveRequestResponse[] = [];
  loading = true;

  constructor(private leaveRequestService: LeaveRequestService) {}

  ngOnInit(): void {
    this.leaveRequestService.getMyBalance().subscribe(balances => {
      this.balances = balances.filter(b => b.remainingDays > 0 || ['Annual', 'Sick', 'Personal'].includes(b.leaveType));
    });

    this.leaveRequestService.getMyRequests().subscribe(requests => {
      this.recentRequests = requests.slice(0, 5);
      this.loading = false;
    });
  }
}
