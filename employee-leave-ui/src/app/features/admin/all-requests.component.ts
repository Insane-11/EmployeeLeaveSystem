import { Component, OnInit } from '@angular/core';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { LeaveRequestResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-all-requests',
  standalone: false,
  templateUrl: './all-requests.component.html'
})
export class AllRequestsComponent implements OnInit {
  requests: LeaveRequestResponse[] = [];
  filtered: LeaveRequestResponse[] = [];
  loading = true;
  filterStatus = '';
  page = 1;
  pageSize = 10;

  constructor(private leaveRequestService: LeaveRequestService) {}

  ngOnInit(): void {
    this.leaveRequestService.getAll().subscribe(requests => {
      this.requests = requests;
      this.applyFilters();
      this.loading = false;
    });
  }

  applyFilters(): void {
    let list = this.requests;
    if (this.filterStatus) {
      list = list.filter(r => r.status === this.filterStatus);
    }
    this.filtered = list;
    this.page = 1;
  }

  get paged(): LeaveRequestResponse[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filtered.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filtered.length / this.pageSize) || 1;
  }

  approve(id: number): void {
    this.leaveRequestService.approve(id, { isApproved: true }).subscribe(() => {
      const r = this.requests.find(x => x.id === id);
      if (r) r.status = 'Approved';
      this.applyFilters();
    });
  }

  reject(id: number): void {
    const comment = prompt('Rejection reason (optional):');
    this.leaveRequestService.reject(id, { isApproved: false, reviewComment: comment || undefined }).subscribe(() => {
      const r = this.requests.find(x => x.id === id);
      if (r) r.status = 'Rejected';
      this.applyFilters();
    });
  }
}
