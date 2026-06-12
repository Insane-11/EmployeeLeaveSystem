# Employee Leave Management System

A full‑stack leave management application built with ASP.NET Core 8 and Angular. Employees submit leave requests, managers review their team's requests, and admins manage users and approvals.

> **Live Demo** → [https://employee-leave-system-ten.vercel.app](https://employee-leave-system-ten.vercel.app)
>
> **API Base** → `https://employeeleavesystem-w4mo.onrender.com`

---

## Features

| Role | Capabilities |
|------|-------------|
| 👤 **Employee** | Submit leave, view history & balance, cancel pending requests |
| 👥 **Manager** | View team leave requests (read‑only dashboard) |
| 🔧 **Admin** | Approve/reject any request, manage users (edit, delete, change roles) |

- JWT authentication with role‑based access control
- Validation — past dates, overlaps, insufficient balance all return clear errors
- Toast notifications & loading spinners on every API call
- Swagger API documentation at `/swagger`
- Unit tests — 33 backend (xUnit + Moq) + 79 frontend (Vitest)

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Backend** | ASP.NET Core 8, Entity Framework Core, SQL Server / PostgreSQL, JWT (Bearer), BCrypt |
| **Frontend** | Angular 22, Bootstrap 5, TypeScript, Vitest |
| **Testing** | xUnit, Moq, EF Core InMemory (backend) — Vitest (frontend) |
| **Deployment** | Render (backend + PostgreSQL), Vercel (frontend) |

---

## Architecture

```
┌──────────────┐     JWT (Bearer)      ┌───────────────┐     EF Core      ┌────────────┐
│   Angular 22  │ ───────────────────▶  │  ASP.NET 8    │ ──────────────▶  │ PostgreSQL │
│  (Vercel)     │ ◀───────────────────  │  Web API      │ ◀──────────────  │ (Render)   │
│               │     JSON responses    │  (Render)     │                  │            │
└──────────────┘                        └───────────────┘                  └────────────┘
```

- RESTful API — stateless JSON communication
- JWT stored in `localStorage`, attached by `HttpInterceptor`
- Route guards restrict pages per role
- Lazy‑loaded employee, manager, and admin modules
- Vercel rewrites `/api/*` to the Render backend (same‑origin in production)

---

## Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full instance)

### 1. Clone

```bash
git clone https://github.com/Insane-11/EmployeeLeaveSystem.git
cd EmployeeLeaveSystem
```

### 2. Backend

Update the connection string in `EmployeeLeaveSystem.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EmployeeLeaveDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

```bash
cd EmployeeLeaveSystem.Api
dotnet restore
dotnet run --urls http://localhost:5000
```

The API starts at `http://localhost:5000`. Swagger at `/swagger`.

> The admin user (`admin@admin.com` / `Admin123!`) is seeded automatically on first run.

### 3. Frontend

```bash
cd employee-leave-ui
npm install
ng serve
```

Open `http://localhost:4200`.

---

## API Endpoints

| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| POST | `/api/auth/login` | — | Login |
| POST | `/api/auth/register` | — | Register (creates Employee role only) |
| GET | `/api/leaverequests` | Employee | My leave requests |
| POST | `/api/leaverequests` | Employee | Create leave request |
| GET | `/api/leaverequests/all` | Admin | All requests |
| PUT | `/api/leaverequests/{id}/approve` | Admin | Approve request |
| PUT | `/api/leaverequests/{id}/reject` | Admin | Reject request |
| GET | `/api/users` | Admin | List all users |
| PUT | `/api/users/{id}` | Admin | Update user |
| DELETE | `/api/users/{id}` | Admin | Delete user |
| GET | `/api/leavebalances` | Employee | My leave balances |
| GET | `/api/leaverequests/team` | Manager | Team requests |

Full documentation at `http://localhost:5000/swagger`.

---

## Project Structure

```
EmployeeLeaveSystem/
├── EmployeeLeaveSystem.Api/        # ASP.NET Core 8 Backend
│   ├── Controllers/                # Auth, LeaveRequests, Users, LeaveBalances
│   ├── Data/                       # DbContext, EF Migrations
│   ├── Models/
│   │   ├── DTOs/                   # Request / Response contracts
│   │   ├── Entities/               # User, LeaveRequest, LeaveBalance, Role
│   │   └── ServiceResult.cs        # Generic response wrapper
│   ├── Services/                   # Auth, User, LeaveRequest, LeaveBalance
│   └── Program.cs                  # Entry point
│
├── EmployeeLeaveSystem.Api.Tests/  # xUnit + Moq + InMemory
│
├── employee-leave-ui/              # Angular 22 Frontend
│   ├── src/app/
│   │   ├── core/
│   │   │   ├── guards/             # AuthGuard, RoleGuard
│   │   │   ├── interceptors/       # JwtInterceptor
│   │   │   ├── layout/             # MainLayout, ToastContainer
│   │   │   ├── models/             # TypeScript interfaces
│   │   │   └── services/           # Auth, User, LeaveRequest, Toast, Loading
│   │   ├── features/
│   │   │   ├── auth/               # Login (with quick-access pills), Register
│   │   │   ├── employee/           # Dashboard, MyRequests, LeaveRequestForm
│   │   │   ├── manager/            # Dashboard (team view)
│   │   │   └── admin/              # Dashboard, AllRequests, UserManagement
│   │   ├── app-module.ts
│   │   └── app-routing-module.ts
│   └── src/environments/           # environment.ts, environment.prod.ts
│
├── DEPLOYMENT.md                   # Deployment guide
└── README.md
```

---

## Testing

```bash
# Backend (33 tests)
cd EmployeeLeaveSystem.Api.Tests
dotnet test

# Frontend (79 tests)
cd employee-leave-ui
npx vitest run
```

---

## Deployment

See **[DEPLOYMENT.md](DEPLOYMENT.md)** for a complete guide to deploy on Render (backend + PostgreSQL) and Vercel (frontend) at zero cost.

---

## License

Personal learning project. Not intended for commercial use.
