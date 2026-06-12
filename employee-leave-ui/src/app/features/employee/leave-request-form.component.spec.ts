import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { LeaveRequestFormComponent } from './leave-request-form.component';
import { LeaveRequestService } from '../../core/services/leave-request.service';

describe('LeaveRequestFormComponent', () => {
  let component: LeaveRequestFormComponent;
  let fixture: ComponentFixture<LeaveRequestFormComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [LeaveRequestFormComponent],
      imports: [FormsModule],
      providers: [
        LeaveRequestService,
        { provide: Router, useValue: { navigate: vi.fn() } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LeaveRequestFormComponent);
    component = fixture.componentInstance;
    const service = TestBed.inject(LeaveRequestService);
    vi.spyOn(service, 'getMyBalance').mockReturnValue(of([
      { leaveType: 'Annual', totalDays: 20, usedDays: 5, remainingDays: 15, year: 2026 },
      { leaveType: 'Sick', totalDays: 10, usedDays: 0, remainingDays: 10, year: 2026 }
    ]));
    fixture.detectChanges();
  });

  it('loads leave balances on init', () => {
    expect(component.balances.length).toBe(2);
  });

  it('computedDays returns correct value', () => {
    component.model.startDate = '2026-07-01';
    component.model.endDate = '2026-07-05';
    expect(component.computedDays).toBe(5);
  });

  it('computedDays returns 1 for same day start/end', () => {
    component.model.startDate = '2026-07-01';
    component.model.endDate = '2026-07-01';
    expect(component.computedDays).toBe(1);
  });

  it('exceedsBalance returns true when days > remaining', () => {
    component.model.leaveType = 'Annual';
    component.model.startDate = '2026-07-01';
    component.model.endDate = '2026-07-20';
    expect(component.exceedsBalance).toBe(true);
  });

  it('exceedsBalance returns false when days <= remaining', () => {
    component.model.leaveType = 'Annual';
    component.model.startDate = '2026-07-01';
    component.model.endDate = '2026-07-05';
    expect(component.exceedsBalance).toBe(false);
  });

  it('isDateValid returns false when start date is in the past', () => {
    component.model.startDate = '2020-01-01';
    component.model.endDate = '2020-01-05';
    expect(component.isDateValid).toBe(false);
  });

  it('returns correct selectedBalance for selected leave type', () => {
    component.model.leaveType = 'Sick';
    expect(component.selectedBalance?.remainingDays).toBe(10);
  });
});
