import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { LeaveBalanceResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-leave-request-form',
  standalone: false,
  templateUrl: './leave-request-form.component.html'
})
export class LeaveRequestFormComponent implements OnInit {
  model = {
    leaveType: 'Annual',
    startDate: '',
    endDate: '',
    reason: ''
  };
  balances: LeaveBalanceResponse[] = [];
  error = '';
  success = false;
  submitting = false;

  constructor(
    private leaveRequestService: LeaveRequestService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.leaveRequestService.getMyBalance().subscribe(balances => {
      this.balances = balances;
    });
  }

  get selectedBalance(): LeaveBalanceResponse | undefined {
    return this.balances.find(b => b.leaveType === this.model.leaveType);
  }

  get computedDays(): number {
    if (!this.model.startDate || !this.model.endDate) return 0;
    const start = new Date(this.model.startDate);
    const end = new Date(this.model.endDate);
    if (end < start) return 0;
    return Math.floor((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24)) + 1;
  }

  get exceedsBalance(): boolean {
    const balance = this.selectedBalance;
    return balance ? this.computedDays > balance.remainingDays : false;
  }

  get isDateValid(): boolean {
    if (!this.model.startDate || !this.model.endDate) return true;
    const start = new Date(this.model.startDate);
    const end = new Date(this.model.endDate);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return start >= today && end >= start;
  }

  get startDateInPast(): boolean {
    if (!this.model.startDate) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return new Date(this.model.startDate) < today;
  }

  get endDateBeforeStart(): boolean {
    if (!this.model.startDate || !this.model.endDate) return false;
    return new Date(this.model.endDate) < new Date(this.model.startDate);
  }

  onSubmit(): void {
    if (this.submitting || !this.isDateValid || this.exceedsBalance) return;
    this.submitting = true;
    this.error = '';

    this.leaveRequestService.create({
      leaveType: this.model.leaveType,
      startDate: this.model.startDate,
      endDate: this.model.endDate,
      reason: this.model.reason || undefined
    }).subscribe({
      next: () => {
        this.success = true;
        setTimeout(() => this.router.navigate(['/employee/my-requests']), 1500);
      },
      error: (err) => {
        this.error = err.error?.message || 'Failed to submit leave request.';
        this.submitting = false;
      }
    });
  }
}
