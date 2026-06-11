export interface UserResponse {
  id: number;
  fullName: string;
  email: string;
  roleName: string;
  department: string | null;
  isActive: boolean;
  managerId: number | null;
  createdAt: string;
}

export interface UpdateUserRequest {
  firstName?: string;
  lastName?: string;
  department?: string;
  isActive?: boolean;
  managerId?: number;
}
