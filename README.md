# Employee Leave Management System

A full‑stack web application that streamlines employee leave requests. Employees submit requests, managers review them, and admins manage users and approvals — all wrapped in a clean Bootstrap‑powered UI.

> **Live Demo** → [https://employee-leave-system.vercel.app](https://employee-leave-system.vercel.app)  

---

## Features

| Role      | Capabilities |
|-----------|-------------|
| 👤 Employee | Submit leave, view history, check remaining balance, cancel pending requests |
| 👥 Manager  | View team leave requests (read‑only) |
| 🔧 Admin    | Approve / reject any request, manage users (edit roles, delete) |

- ✅ JWT authentication with role‑based access control  
- ✅ Real‑time validation — past dates, overlaps, insufficient balance all return clear error messages  
- ✅ Toast notifications & loading spinner on every API call  
- ✅ Swagger API documentation at `/swagger`  

---

## Tech Stack

| Layer      | Technology |
|-----------|-----------|
| **Backend**   | ASP.NET Core 8, Entity Framework Core, SQL Server, JWT (Bearer), BCrypt |
| **Frontend**  | Angular 17, Bootstrap 5, TypeScript |
| **Tooling**   | Git, Swagger / Swashbuckle, Visual Studio Code |

---

## Architecture

```
┌─────────────┐       JWT (Bearer)       ┌──────────────┐       EF Core       ┌──────────┐
│  Angular 17  │ ──────────────────────▶  │  ASP.NET 8   │ ────────────────▶  │ SQL      │
│  (localhost  │ ◀──────────────────────  │  Web API     │ ◀────────────────  │ Server   │
│   :4200)     │      JSON responses      │  (:5131)     │                    │          │
└─────────────┘                           └──────────────┘                    └──────────┘
```

- **RESTful API** — all communication is stateless JSON  
- **JWT** — token stored in `localStorage`, attached by `HttpInterceptor`  
- **Role Guards** — Angular route guards restrict pages per role  
- **Lazy loading** — Employee, Manager, Admin modules load on demand  

---

## Local Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full instance)
- [Git](https://git-scm.com/)

### 1. Clone & configure

```bash
git clone https://github.com/Insane-11/EmployeeLeaveSystem.git
cd EmployeeLeaveSystem
```

Update the connection string in `EmployeeLeaveSystem.Api/appsettings.json` if needed:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=EmployeeLeaveDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 2. Backend

```bash
cd EmployeeLeaveSystem.Api
dotnet restore
dotnet ef database update
dotnet run --urls http://localhost:5131
```

The API starts at `http://localhost:5131`. Swagger is at `http://localhost:5131/swagger`.

### 3. Frontend

```bash
cd employee-leave-ui
npm install
ng serve --port 4200
```

Open `http://localhost:4200` in your browser.

---

## API Documentation

With the backend running, visit:

▶ **http://localhost:5131/swagger**

Swagger UI lists every endpoint with request/response schemas and lets you execute calls directly.

---

## Project Structure

```
EmployeeLeaveSystem/
├── EmployeeLeaveSystem.Api/        # ASP.NET Core 8 Backend
│   ├── Controllers/                # Auth, LeaveRequests, Users, LeaveBalances
│   ├── Data/                       # DbContext, EF Migrations
│   ├── Middleware/                 # (future) custom middleware
│   ├── Models/
│   │   ├── DTOs/                   # Request / Response contracts
│   │   ├── Entities/               # User, LeaveRequest, LeaveBalance, Role
│   │   ├── Enums/                  # LeaveType, LeaveRequestStatus
│   │   └── ServiceResult.cs        # Generic success/error wrapper
│   ├── Services/                   # Auth, User, LeaveRequest, LeaveBalance
│   ├── Program.cs                  # App entry point
│   └── appsettings.json            # Connection string, JWT config
│
├── employee-leave-ui/              # Angular 17 Frontend
│   ├── src/app/
│   │   ├── core/
│   │   │   ├── guards/             # AuthGuard, RoleGuard
│   │   │   ├── interceptors/       # JwtInterceptor (auto-attach token, toast errors)
│   │   │   ├── layout/             # MainLayout, ToastContainer
│   │   │   ├── models/             # TypeScript interfaces
│   │   │   └── services/           # Auth, User, LeaveRequest, Toast, Loading
│   │   ├── features/
│   │   │   ├── auth/               # LoginComponent, RegisterComponent
│   │   │   ├── employee/           # Dashboard, MyRequests, LeaveRequestForm
│   │   │   ├── manager/            # Dashboard (team view)
│   │   │   └── admin/              # Dashboard, AllRequests, UserManagement
│   │   ├── app-module.ts
│   │   └── app-routing-module.ts
│   └── angular.json
│
├── README.md
└── DEPLOYMENT.md                   # Free deployment guide
```

---

## Deployment

See **[DEPLOYMENT.md](DEPLOYMENT.md)** for a complete step‑by‑step guide to deploy the backend on Render (free PostgreSQL) and the frontend on Vercel — at zero cost.

---

## License

This is a personal learning project. Not intended for commercial use.
