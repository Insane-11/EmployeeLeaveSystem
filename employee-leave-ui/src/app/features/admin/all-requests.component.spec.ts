import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { AllRequestsComponent } from './all-requests.component';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { ToastService } from '../../core/services/toast.service';

describe('AllRequestsComponent', () => {
  let component: AllRequestsComponent;
  let fixture: ComponentFixture<AllRequestsComponent>;
  let leaveRequestService: LeaveRequestService;

  const mockRequests: any[] = Array.from({ length: 8 }, (_, i) => ({
    id: i + 1, employeeName: 'User', leaveType: 'Annual', status: i < 3 ? 'Pending' : 'Approved',
    startDate: '2026-07-01', endDate: '2026-07-02', durationDays: 2, employeeId: 1, submittedAt: '2026-06-01',
    reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null
  }));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AllRequestsComponent],
      imports: [FormsModule],
      providers: [LeaveRequestService, ToastService]
    }).compileComponents();

    fixture = TestBed.createComponent(AllRequestsComponent);
    component = fixture.componentInstance;
    leaveRequestService = TestBed.inject(LeaveRequestService);
    vi.spyOn(leaveRequestService, 'getAll').mockReturnValue(of(mockRequests));
    fixture.detectChanges();
  });

  it('loads all requests on init', () => {
    expect(component.requests.length).toBe(8);
  });

  it('approve calls service.approve', () => {
    const spy = vi.spyOn(leaveRequestService, 'approve').mockReturnValue(of({ message: 'ok' }));
    component.approve(3);
    expect(spy).toHaveBeenCalledWith(3, { isApproved: true });
  });

  it('reject calls service.reject', () => {
    const spy = vi.spyOn(leaveRequestService, 'reject').mockReturnValue(of({ message: 'ok' }));
    component.reject(4);
    expect(spy).toHaveBeenCalledWith(4, { isApproved: false, reviewComment: undefined });
  });

  it('paginates with default page size of 10', () => {
    expect(component.paged.length).toBe(8);
    expect(component.totalPages).toBe(1);
  });
});
