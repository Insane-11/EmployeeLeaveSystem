import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { ManagerDashboardComponent } from './manager-dashboard.component';
import { UserService } from '../../core/services/user.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';

describe('ManagerDashboardComponent', () => {
  let component: ManagerDashboardComponent;
  let fixture: ComponentFixture<ManagerDashboardComponent>;
  let userService: UserService;
  let leaveRequestService: LeaveRequestService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ManagerDashboardComponent],
      providers: [UserService, LeaveRequestService]
    }).compileComponents();

    fixture = TestBed.createComponent(ManagerDashboardComponent);
    component = fixture.componentInstance;
    userService = TestBed.inject(UserService);
    leaveRequestService = TestBed.inject(LeaveRequestService);
  });

  it('loads team members and requests on init', () => {
    vi.spyOn(userService, 'getTeam').mockReturnValue(of([
      { id: 2, fullName: 'Jane Dev', email: 'jane@test.com', roleName: 'Employee', isActive: true, createdAt: '2026-01-01', department: null, managerId: 1 }
    ]));
    vi.spyOn(leaveRequestService, 'getTeamRequests').mockReturnValue(of([
      { id: 1, employeeName: 'Jane Dev', leaveType: 'Annual', status: 'Pending', startDate: '2026-07-01', endDate: '2026-07-03', durationDays: 3, employeeId: 2, submittedAt: '2026-06-01', reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null }
    ]));
    fixture.detectChanges();
    expect(component.teamMembers.length).toBe(1);
    expect(component.teamRequests.length).toBe(1);
    expect(component.pendingCount).toBe(1);
  });
});
