import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { ManagerDashboardComponent } from './manager-dashboard.component';

const routes: Routes = [
  { path: 'dashboard', component: ManagerDashboardComponent },
  { path: 'team-requests', component: ManagerDashboardComponent },
  { path: '', redirectTo: 'dashboard', pathMatch: 'full' }
];

@NgModule({
  declarations: [
    ManagerDashboardComponent
  ],
  imports: [CommonModule, FormsModule, RouterModule.forChild(routes)]
})
export class ManagerModule {}
