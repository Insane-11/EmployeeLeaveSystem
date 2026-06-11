import { Component, OnInit } from '@angular/core';
import { UserService } from '../../core/services/user.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { UserResponse } from '../../core/models/user.model';
import { LeaveRequestResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-manager-dashboard',
  standalone: false,
  templateUrl: './manager-dashboard.component.html'
})
export class ManagerDashboardComponent implements OnInit {
  teamMembers: UserResponse[] = [];
  teamRequests: LeaveRequestResponse[] = [];
  loading = true;

  constructor(
    private userService: UserService,
    private leaveRequestService: LeaveRequestService
  ) {}

  ngOnInit(): void {
    this.userService.getTeam().subscribe(members => {
      this.teamMembers = members;
    });

    this.leaveRequestService.getTeamRequests().subscribe(requests => {
      this.teamRequests = requests;
      this.loading = false;
    });
  }

  get pendingCount(): number {
    return this.teamRequests.filter(r => r.status === 'Pending').length;
  }
}
