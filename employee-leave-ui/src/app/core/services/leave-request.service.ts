import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateLeaveRequest,
  UpdateLeaveRequest,
  ReviewLeaveRequest,
  LeaveRequestResponse,
  LeaveBalanceResponse
} from '../models/leave-request.model';

@Injectable({ providedIn: 'root' })
export class LeaveRequestService {
  private readonly baseUrl = '/api/leaverequests';

  constructor(private http: HttpClient) {}

  create(request: CreateLeaveRequest): Observable<LeaveRequestResponse> {
    return this.http.post<LeaveRequestResponse>(this.baseUrl, request);
  }

  getMyRequests(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(this.baseUrl);
  }

  getById(id: number): Observable<LeaveRequestResponse> {
    return this.http.get<LeaveRequestResponse>(`${this.baseUrl}/${id}`);
  }

  update(id: number, request: UpdateLeaveRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.baseUrl}/${id}`, request);
  }

  cancel(id: number): Observable<{ message: string }> {
    return this.http.delete<{ message: string }>(`${this.baseUrl}/${id}`);
  }

  getTeamRequests(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(`${this.baseUrl}/team`);
  }

  getAll(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(`${this.baseUrl}/all`);
  }

  getPending(): Observable<LeaveRequestResponse[]> {
    return this.http.get<LeaveRequestResponse[]>(`${this.baseUrl}/pending`);
  }

  approve(id: number, review: ReviewLeaveRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.baseUrl}/${id}/approve`, review);
  }

  reject(id: number, review: ReviewLeaveRequest): Observable<{ message: string }> {
    return this.http.put<{ message: string }>(`${this.baseUrl}/${id}/reject`, review);
  }

  getMyBalance(year?: number): Observable<LeaveBalanceResponse[]> {
    const params = year ? `?year=${year}` : '';
    return this.http.get<LeaveBalanceResponse[]>(`/api/leavebalance${params}`);
  }

  getUserBalance(userId: number, year?: number): Observable<LeaveBalanceResponse[]> {
    const params = year ? `?year=${year}` : '';
    return this.http.get<LeaveBalanceResponse[]>(`/api/leavebalance/${userId}${params}`);
  }
}
