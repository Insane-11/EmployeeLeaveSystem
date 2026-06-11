import { Component, OnInit } from '@angular/core';
import { LeaveRequestService } from '../../core/services/leave-request.service';
import { LeaveRequestResponse } from '../../core/models/leave-request.model';

@Component({
  selector: 'app-my-requests',
  standalone: false,
  templateUrl: './my-requests.component.html'
})
export class MyRequestsComponent implements OnInit {
  allRequests: LeaveRequestResponse[] = [];
  filteredRequests: LeaveRequestResponse[] = [];
  loading = true;
  filterStatus = '';
  page = 1;
  pageSize = 5;

  constructor(private leaveRequestService: LeaveRequestService) {}

  ngOnInit(): void {
    this.leaveRequestService.getMyRequests().subscribe(requests => {
      this.allRequests = requests;
      this.applyFilters();
      this.loading = false;
    });
  }

  applyFilters(): void {
    let filtered = this.allRequests;
    if (this.filterStatus) {
      filtered = filtered.filter(r => r.status === this.filterStatus);
    }
    this.filteredRequests = filtered;
    this.page = 1;
  }

  get pagedRequests(): LeaveRequestResponse[] {
    const start = (this.page - 1) * this.pageSize;
    return this.filteredRequests.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.ceil(this.filteredRequests.length / this.pageSize) || 1;
  }

  cancelRequest(id: number): void {
    if (confirm('Cancel this leave request?')) {
      this.leaveRequestService.cancel(id).subscribe(() => {
        const req = this.allRequests.find(r => r.id === id);
        if (req) req.status = 'Cancelled';
        this.applyFilters();
      });
    }
  }
}
