import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { LeaveRequestService } from './leave-request.service';

describe('LeaveRequestService', () => {
  let service: LeaveRequestService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [LeaveRequestService]
    });
    service = TestBed.inject(LeaveRequestService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('create POSTs to /api/LeaveRequests', () => {
    const reqBody = { leaveType: 'Annual', startDate: '2026-07-01', endDate: '2026-07-05', reason: 'Vacation' };
    service.create(reqBody).subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(reqBody);
    req.flush({});
  });

  it('getMyRequests GETs /api/LeaveRequests', () => {
    service.getMyRequests().subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('cancel DELETEs /api/LeaveRequests/{id}', () => {
    service.cancel(5).subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests/5');
    expect(req.request.method).toBe('DELETE');
    req.flush({});
  });

  it('approve PUTs /api/LeaveRequests/{id}/approve', () => {
    const review = { isApproved: true, reviewComment: 'Looks good' };
    service.approve(3, review).subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests/3/approve');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(review);
    req.flush({});
  });

  it('reject PUTs /api/LeaveRequests/{id}/reject', () => {
    const review = { isApproved: false, reviewComment: 'Not enough info' };
    service.reject(4, review).subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests/4/reject');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(review);
    req.flush({});
  });

  it('getAll GETs /api/LeaveRequests/all', () => {
    service.getAll().subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests/all');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getTeamRequests GETs /api/LeaveRequests/team', () => {
    service.getTeamRequests().subscribe();
    const req = httpMock.expectOne('/api/LeaveRequests/team');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });

  it('getMyBalance GETs /api/LeaveBalances', () => {
    service.getMyBalance().subscribe();
    const req = httpMock.expectOne('/api/LeaveBalances');
    expect(req.request.method).toBe('GET');
    req.flush([]);
  });
});
