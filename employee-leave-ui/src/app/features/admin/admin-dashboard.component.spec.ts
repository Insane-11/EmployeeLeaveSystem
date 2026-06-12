import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { AdminDashboardComponent } from './admin-dashboard.component';
import { UserService } from '../../core/services/user.service';
import { LeaveRequestService } from '../../core/services/leave-request.service';

describe('AdminDashboardComponent', () => {
  let component: AdminDashboardComponent;
  let fixture: ComponentFixture<AdminDashboardComponent>;
  let userService: UserService;
  let leaveRequestService: LeaveRequestService;

  beforeEach(async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      declarations: [AdminDashboardComponent],
      imports: [HttpClientTestingModule],
      providers: [UserService, LeaveRequestService]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminDashboardComponent);
    component = fixture.componentInstance;
    userService = TestBed.inject(UserService);
    leaveRequestService = TestBed.inject(LeaveRequestService);
  });

  it('loads users and requests on init', () => {
    vi.spyOn(userService, 'getAll').mockReturnValue(of([
      { id: 1, fullName: 'A', email: 'a@a.com', roleName: 'Admin', isActive: true, createdAt: '2026-01-01', department: null, managerId: null }
    ]));
    vi.spyOn(leaveRequestService, 'getAll').mockReturnValue(of([
      { id: 1, employeeName: 'A', leaveType: 'Annual', status: 'Pending', startDate: '2026-07-01', endDate: '2026-07-02', durationDays: 2, employeeId: 1, submittedAt: '2026-06-01', reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null }
    ]));
    vi.spyOn(leaveRequestService, 'getPending').mockReturnValue(of([
      { id: 1, employeeName: 'A', leaveType: 'Annual', status: 'Pending', startDate: '2026-07-01', endDate: '2026-07-02', durationDays: 2, employeeId: 1, submittedAt: '2026-06-01', reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null }
    ]));
    fixture.detectChanges();
    expect(component.totalUsers).toBe(1);
    expect(component.pendingApprovals).toBe(1);
  });
});
