import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';
import { RoleGuard } from './core/guards/role.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { RegisterComponent } from './features/auth/register/register.component';
import { MainLayoutComponent } from './core/layout/main-layout.component';

const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  {
    path: '',
    component: MainLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      {
        path: 'employee',
        canActivate: [RoleGuard],
        data: { roles: ['Employee'] },
        loadChildren: () =>
          import('./features/employee/employee.module').then(m => m.EmployeeModule)
      },
      {
        path: 'manager',
        canActivate: [RoleGuard],
        data: { roles: ['Manager'] },
        loadChildren: () =>
          import('./features/manager/manager.module').then(m => m.ManagerModule)
      },
      {
        path: 'admin',
        canActivate: [RoleGuard],
        data: { roles: ['Admin'] },
        loadChildren: () =>
          import('./features/admin/admin.module').then(m => m.AdminModule)
      },
    ]
  },
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: '**', redirectTo: '/login' }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
