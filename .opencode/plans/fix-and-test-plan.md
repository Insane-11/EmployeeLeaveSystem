# EmployeeLeaveSystem — Fix & Test Plan

## Phase 1: Critical Bug Fixes

### Fix C1: EF Core `.Select()` with custom method calls

**Problem:** `UserService.cs` and `LeaveRequestService.cs` call `MapToResponse()` inside EF Core `.Select()`. EF cannot translate C# methods to SQL — throws `InvalidOperationException` at runtime.

**Fix:** Materialize the query first with `ToListAsync()`, then `Select()` in-memory.

#### UserService.cs — 2 changes

**Change 1** (GetAllUsersAsync, line 21):
```csharp
// BEFORE:
return await _context.Users
    .Include(u => u.Role)
    .Select(u => MapToResponse(u))
    .ToListAsync();

// AFTER:
var users = await _context.Users
    .Include(u => u.Role)
    .ToListAsync();
return users.Select(u => MapToResponse(u)).ToList();
```

**Change 2** (GetTeamMembersAsync, line 81):
```csharp
// BEFORE:
return await _context.Users
    .Include(u => u.Role)
    .Where(u => u.ManagerId == _currentUser.UserId)
    .Select(u => MapToResponse(u))
    .ToListAsync();

// AFTER:
var users = await _context.Users
    .Include(u => u.Role)
    .Where(u => u.ManagerId == _currentUser.UserId)
    .ToListAsync();
return users.Select(u => MapToResponse(u)).ToList();
```

#### LeaveRequestService.cs — 5 changes

**Change 3** (GetMyLeaveRequestsAsync, line 76):
```csharp
public async Task<List<LeaveRequestResponse>> GetMyLeaveRequestsAsync(int employeeId)
{
    var requests = await _context.LeaveRequests
        .Include(l => l.Employee)
        .Include(l => l.Reviewer)
        .Include(l => l.Status)
        .Where(l => l.EmployeeId == employeeId)
        .OrderByDescending(l => l.SubmittedAt)
        .ToListAsync();
    return requests.Select(l => MapToResponse(l)).ToList();
}
```

**Change 4** (GetTeamLeaveRequestsAsync, line 170):
```csharp
public async Task<List<LeaveRequestResponse>> GetTeamLeaveRequestsAsync(int managerId)
{
    var requests = await _context.LeaveRequests
        .Include(l => l.Employee)
        .Include(l => l.Reviewer)
        .Include(l => l.Status)
        .Where(l => l.Employee.ManagerId == managerId)
        .OrderByDescending(l => l.SubmittedAt)
        .ToListAsync();
    return requests.Select(l => MapToResponse(l)).ToList();
}
```

**Change 5** (GetAllLeaveRequestsAsync, line 182):
```csharp
public async Task<List<LeaveRequestResponse>> GetAllLeaveRequestsAsync()
{
    var requests = await _context.LeaveRequests
        .Include(l => l.Employee)
        .Include(l => l.Reviewer)
        .Include(l => l.Status)
        .OrderByDescending(l => l.SubmittedAt)
        .ToListAsync();
    return requests.Select(l => MapToResponse(l)).ToList();
}
```

**Change 6** (GetPendingLeaveRequestsAsync, line 193):
```csharp
public async Task<List<LeaveRequestResponse>> GetPendingLeaveRequestsAsync()
{
    var requests = await _context.LeaveRequests
        .Include(l => l.Employee)
        .Include(l => l.Reviewer)
        .Include(l => l.Status)
        .Where(l => l.StatusId == (int)LeaveRequestStatusEnum.Pending)
        .OrderByDescending(l => l.SubmittedAt)
        .ToListAsync();
    return requests.Select(l => MapToResponse(l)).ToList();
}
```

### Fix C2: Add DeductBalanceAsync call in ApproveLeaveRequestAsync

**File:** `LeaveRequestService.cs` line 218, after `leaveRequest.ReviewedAt = DateTime.UtcNow;`

Add:
```csharp
await _leaveBalanceService.DeductBalanceAsync(
    leaveRequest.EmployeeId,
    leaveRequest.LeaveType,
    leaveRequest.DurationDays,
    leaveRequest.SubmittedAt.Year);
```

### Fix C3: Remove unused using in Role.cs

**File:** `Models\Entities\Role.cs` line 2 — remove `using System.ComponentModel.DataAnnotations.Schema;`

---

## Phase 2: UI/UX Fixes

### Fix M1: Dead loading spinner

**File:** `employee-leave-ui\src\app\app.html` line 5

Change:
```html
<div class="spinner-overlay" *ngIf="false">
```
To:
```html
<div class="spinner-overlay" *ngIf="loadingService.loading | async">
```

### Fix M2: JWT interceptor counter mismatch

**File:** `employee-leave-ui\src\app\core\interceptors\jwt.interceptor.ts`

The issue: `loadingService.show()` is called conditionally (non-skip paths only) but `loadingService.hide()` runs in `finalize` for ALL requests.

Fix: Track whether `show()` was called, and only `hide()` if it was:

```typescript
const isAuthPath = req.url.includes('/api/auth/login') || req.url.includes('/api/auth/register');
let loadingShown = false;

if (!isAuthPath) {
  this.loadingService.show();
  loadingShown = true;
}

return next.handle(req).pipe(
  timeout(15000),
  finalize(() => {
    if (loadingShown) this.loadingService.hide();
  }),
  catchError(err => { ... })
);
```

---

## Phase 3: Backend Unit Tests (xUnit)

### Create test project

```bash
cd EmployeeLeaveSystem
dotnet new xunit -n EmployeeLeaveSystem.Api.Tests
cd EmployeeLeaveSystem.Api.Tests
dotnet add reference ../EmployeeLeaveSystem.Api/EmployeeLeaveSystem.Api.csproj
dotnet add package Moq
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

### AuthService Tests

```csharp
// Tests\AuthServiceTests.cs
public class AuthServiceTests
{
    [Fact]
    public async Task Login_ValidCredentials_ReturnsLoginResponse() { /* ... */ }
    [Fact]
    public async Task Login_InvalidPassword_ReturnsNull() { /* ... */ }
    [Fact]
    public async Task Login_InactiveUser_ReturnsNull() { /* ... */ }
    [Fact]
    public async Task Register_NewUser_CreatesSuccessfully() { /* ... */ }
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsFalse() { /* ... */ }
    [Fact]
    public async Task Register_DefaultRoleIsEmployee() { /* ... */ }
}
```

### LeaveRequestService Tests

```csharp
// Tests\LeaveRequestServiceTests.cs
public class LeaveRequestServiceTests
{
    [Fact]
    public async Task CreateLeaveRequest_PastStartDate_ReturnsFailure() { /* ... */ }
    [Fact]
    public async Task CreateLeaveRequest_EndBeforeStart_ReturnsFailure() { /* ... */ }
    [Fact]
    public async Task CreateLeaveRequest_OverlappingRequest_ReturnsFailure() { /* ... */ }
    [Fact]
    public async Task CreateLeaveRequest_ValidRequest_CreatesSuccessfully() { /* ... */ }
    [Fact]
    public async Task ApproveLeaveRequest_ValidRequest_UpdatesStatusAndDeductsBalance() { /* ... */ }
    [Fact]
    public async Task ApproveLeaveRequest_NonPendingRequest_ReturnsFalse() { /* ... */ }
    [Fact]
    public async Task CancelLeaveRequest_PendingRequest_CancelsSuccessfully() { /* ... */ }
    [Fact]
    public async Task CancelLeaveRequest_ApprovedRequest_ReturnsFalse() { /* ... */ }
    [Fact]
    public async Task GetMyLeaveRequests_ReturnsOnlyOwnRequests() { /* ... */ }
}
```

---

## Phase 4: Frontend Unit Tests (Vitest)

### Service Tests

```typescript
// src/app/core/services/auth.service.spec.ts
describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [AuthService]
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  it('login POSTs to /api/auth/login', () => { /* ... */ });
  it('login stores token in localStorage', () => { /* ... */ });
  it('register POSTs to /api/auth/register', () => { /* ... */ });
  it('isLoggedIn returns false without token', () => { /* ... */ });
});
```

### Component Tests

```typescript
// src/app/features/auth/login/login.component.spec.ts
describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [FormsModule, RouterTestingModule],
      providers: [AuthService, ToastService]
    }).compileComponents();
  });

  it('renders email and password fields', () => { /* ... */ });
  it('valid form calls authService.login()', () => { /* ... */ });
  it('invalid email shows validation error', () => { /* ... */ });
});
```

### Interceptor Tests

```typescript
// src/app/core/interceptors/jwt.interceptor.spec.ts
describe('JwtInterceptor', () => {
  it('attaches bearer token', () => { /* ... */ });
  it('skips auth header for login', () => { /* ... */ });
  it('handles 401 by clearing session', () => { /* ... */ });
});
```

---

## Phase 5: Local Integration Test

```bash
# 1. Start backend
cd EmployeeLeaveSystem.Api
dotnet run

# 2. In another terminal, test API
curl -X POST http://localhost:5131/api/auth/register ^
  -H "Content-Type: application/json" ^
  -d "{\"firstName\":\"Admin\",\"lastName\":\"User\",\"email\":\"admin@test.com\",\"password\":\"Admin123!\",\"roleId\":1}"

curl -X POST http://localhost:5131/api/auth/login ^
  -H "Content-Type: application/json" ^
  -d "{\"email\":\"admin@test.com\",\"password\":\"Admin123!\"}"

# 3. Start frontend
cd employee-leave-ui
npm start

# 4. Open http://localhost:4200
# 5. Test full flow: Register -> Login -> Dashboard -> Create Request -> View Requests
```

---

## Phase 6: Git & Deploy

```bash
cd EmployeeLeaveSystem
git add -A
git commit -m "fix: critical EF Core query bugs, balance deduction, and add comprehensive test suite"
git push origin main
```
