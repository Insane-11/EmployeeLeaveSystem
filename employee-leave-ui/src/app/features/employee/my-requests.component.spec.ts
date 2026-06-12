import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { of } from 'rxjs';
import { MyRequestsComponent } from './my-requests.component';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { ToastService } from '../../core/services/toast.service';

describe('MyRequestsComponent', () => {
  let component: MyRequestsComponent;
  let fixture: ComponentFixture<MyRequestsComponent>;
  let leaveRequestService: LeaveRequestService;

  const mockRequests: any[] = Array.from({ length: 12 }, (_, i) => ({
    id: i + 1, leaveType: 'Annual', status: i === 0 ? 'Pending' : 'Approved',
    startDate: '2026-07-01', endDate: '2026-07-02', durationDays: 2,
    employeeName: 'Me', employeeId: 1, submittedAt: '2026-06-01',
    reason: null, reviewerId: null, reviewerName: null, reviewComment: null, reviewedAt: null
  }));

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [MyRequestsComponent],
      imports: [FormsModule],
      providers: [LeaveRequestService, ToastService]
    }).compileComponents();

    fixture = TestBed.createComponent(MyRequestsComponent);
    component = fixture.componentInstance;
    leaveRequestService = TestBed.inject(LeaveRequestService);
    vi.spyOn(leaveRequestService, 'getMyRequests').mockReturnValue(of(mockRequests));
    fixture.detectChanges();
  });

  it('loads requests on init', () => {
    expect(component.allRequests.length).toBe(12);
  });

  it('paginates with 5 items per page', () => {
    expect(component.pagedRequests.length).toBe(5);
    expect(component.totalPages).toBe(3);
  });

  it('filterStatus filters requests', () => {
    component.filterStatus = 'Approved';
    component.applyFilters();
    expect(component.filteredRequests.every(r => r.status === 'Approved')).toBe(true);
  });
});
