import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { EmployeeDashboardComponent } from './employee-dashboard.component';
import { MyRequestsComponent } from './my-requests.component';
import { LeaveRequestFormComponent } from './leave-request-form.component';

const routes: Routes = [
  { path: 'dashboard', component: EmployeeDashboardComponent },
  { path: 'my-requests', component: MyRequestsComponent },
  { path: 'new-request', component: LeaveRequestFormComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    EmployeeDashboardComponent,
    MyRequestsComponent,
    LeaveRequestFormComponent
  ],
  imports: [CommonModule, FormsModule, RouterModule.forChild(routes)]
})
export class EmployeeModule {}
