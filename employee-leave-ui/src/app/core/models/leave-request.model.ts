export interface CreateLeaveRequest {
  leaveType: string;
  startDate: string;
  endDate: string;
  reason?: string;
}

export interface UpdateLeaveRequest {
  leaveType: string;
  startDate: string;
  endDate: string;
  reason?: string;
}

export interface ReviewLeaveRequest {
  isApproved: boolean;
  reviewComment?: string;
}

export interface LeaveRequestResponse {
  id: number;
  employeeId: number;
  employeeName: string;
  leaveType: string;
  startDate: string;
  endDate: string;
  durationDays: number;
  reason: string | null;
  status: string;
  reviewerId: number | null;
  reviewerName: string | null;
  reviewComment: string | null;
  submittedAt: string;
  reviewedAt: string | null;
}

export interface LeaveBalanceResponse {
  leaveType: string;
  totalDays: number;
  usedDays: number;
  remainingDays: number;
  year: number;
}
