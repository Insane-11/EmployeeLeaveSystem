import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { AdminDashboardComponent } from './admin-dashboard.component';
import { UserManagementComponent } from './user-management.component';
import { AllRequestsComponent } from './all-requests.component';

const routes: Routes = [
  { path: 'dashboard', component: AdminDashboardComponent },
  { path: 'users', component: UserManagementComponent },
  { path: 'all-requests', component: AllRequestsComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    AdminDashboardComponent,
    UserManagementComponent,
    AllRequestsComponent
  ],
  imports: [CommonModule, FormsModule, RouterModule.forChild(routes)]
})
export class AdminModule {}
