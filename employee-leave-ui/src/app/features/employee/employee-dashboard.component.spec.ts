import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { EmployeeDashboardComponent } from './employee-dashboard.component';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { ToastService } from '../../core/services/toast.service';

describe('EmployeeDashboardComponent', () => {
  let component: EmployeeDashboardComponent;
  let fixture: ComponentFixture<EmployeeDashboardComponent>;
  let leaveRequestService: LeaveRequestService;

  beforeEach(async () => {
    TestBed.resetTestingModule();
    await TestBed.configureTestingModule({
      declarations: [EmployeeDashboardComponent],
      imports: [HttpClientTestingModule],
      providers: [LeaveRequestService, ToastService]
    }).compileComponents();

    fixture = TestBed.createComponent(EmployeeDashboardComponent);
    component = fixture.componentInstance;
    leaveRequestService = TestBed.inject(LeaveRequestService);
  });

  it('loads balances on init', () => {
    vi.spyOn(leaveRequestService, 'getMyBalance').mockReturnValue(of([
      { leaveType: 'Annual', totalDays: 20, usedDays: 0, remainingDays: 20, year: 2026 }
    ]));
    vi.spyOn(leaveRequestService, 'getMyRequests').mockReturnValue(of([]));
    fixture.detectChanges();
    expect(component.balances.length).toBe(1);
    expect(component.balances[0].leaveType).toBe('Annual');
  });

  it('loads recent requests on init', () => {
    vi.spyOn(leaveRequestService, 'getMyBalance').mockReturnValue(of([]));
    vi.spyOn(leaveRequestService, 'getMyRequests').mockReturnValue(of([
      { id: 1, leaveType: 'Sick', status: 'Pending', startDate: '2026-07-01', endDate: '2026-07-02', durationDays: 2, employeeName: 'Me', employeeId: 1, submittedAt: '2026-06-01', reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null }
    ]));
    fixture.detectChanges();
    expect(component.recentRequests.length).toBe(1);
    expect(component.recentRequests[0].leaveType).toBe('Sick');
  });

  it('filters to last 5 requests', () => {
    const manyRequests: any[] = Array.from({ length: 10 }, (_, i) => ({
      id: i + 1, leaveType: 'Annual', status: 'Pending',
      startDate: '2026-07-01', endDate: '2026-07-02', durationDays: 2,
      employeeName: 'Me', employeeId: 1, submittedAt: '2026-06-01',
      reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null
    }));
    vi.spyOn(leaveRequestService, 'getMyBalance').mockReturnValue(of([]));
    vi.spyOn(leaveRequestService, 'getMyRequests').mockReturnValue(of(manyRequests));
    fixture.detectChanges();
    expect(component.recentRequests.length).toBe(5);
  });
});
