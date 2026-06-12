import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { UserManagementComponent } from './user-management.component';
import { UserService } from '../../core/services/user.service';

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;
  let userService: UserService;

  const mockUsers: any[] = [
    { id: 1, fullName: 'Alice Smith', email: 'alice@test.com', roleName: 'Admin', isActive: true, createdAt: '2026-01-01', department: null, managerId: null },
    { id: 2, fullName: 'Bob Jones', email: 'bob@test.com', roleName: 'Employee', isActive: true, department: 'Engineering', managerId: null, createdAt: '2026-01-02' }
  ];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [UserManagementComponent],
      providers: [UserService]
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
    userService = TestBed.inject(UserService);
    vi.spyOn(userService, 'getAll').mockReturnValue(of(mockUsers));
    fixture.detectChanges();
  });

  it('loads users on init', () => {
    expect(component.users.length).toBe(2);
  });

  it('openEdit splits fullName into first/last', () => {
    component.openEdit(mockUsers[0]);
    expect(component.editModel.firstName).toBe('Alice');
    expect(component.editModel.lastName).toBe('Smith');
  });

  it('openEdit handles names with multiple parts', () => {
    component.openEdit(mockUsers[1]);
    expect(component.editModel.firstName).toBe('Bob');
    expect(component.editModel.lastName).toBe('Jones');
  });

  it('saveEdit calls service.update', () => {
    const spy = vi.spyOn(userService, 'update').mockReturnValue(of({ message: 'ok' }));
    component.openEdit(mockUsers[1]);
    component.saveEdit();
    expect(spy).toHaveBeenCalledWith(2, expect.objectContaining({ firstName: 'Bob' }));
  });

  it('saveEdit clears editUser on success', () => {
    vi.spyOn(userService, 'update').mockReturnValue(of({ message: 'ok' }));
    component.openEdit(mockUsers[1]);
    component.saveEdit();
    expect(component.editUser).toBeNull();
  });
});
