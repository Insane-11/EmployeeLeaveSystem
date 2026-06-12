# Deployment Guide

Deploy the Employee Leave Management System at **zero cost** using **Render** (backend + PostgreSQL) and **Vercel** (frontend).

## Live URLs

| Service | URL |
|---------|-----|
| **Frontend** (Vercel) | `https://employee-leave-system-ten.vercel.app` |
| **Backend** (Render) | `https://employeeleavesystem-w4mo.onrender.com` |

> ⚠️ Render's free tier spins down after 15 minutes of inactivity. The first request after idle takes 30–60 seconds to wake up.

---

## Table of Contents

1. [Prepare GitHub Repository](#1-prepare-github-repository)
2. [Backend — PostgreSQL Migration](#2-backend--postgresql-migration)
3. [Backend — Render Web Service](#3-backend--render-web-service)
4. [Frontend — Vercel Deployment](#4-frontend--vercel-deployment)

---

## 1. Prepare GitHub Repository

Push your code to a **public** GitHub repository so Render and Vercel can access it.

```bash
git remote add origin https://github.com/Insane-11/EmployeeLeaveSystem.git
git push -u origin main
```

---

## 2. Backend — PostgreSQL Migration

Render's free tier doesn't support SQL Server. Switch to PostgreSQL via the `Npgsql` provider.

### 2a. Add the Npgsql package

```bash
cd EmployeeLeaveSystem.Api
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 2b. Update `Program.cs`

```csharp
// Replace:
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

// With:
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
```

### 2c. Generate EF migrations for PostgreSQL

```bash
rm -rf Migrations
dotnet ef migrations add InitialCreate
```

### 2d. Update `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EmployeeLeaveDb;Username=postgres;Password=your_password"
  },
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-please-change-me",
    "Issuer": "EmployeeLeaveSystem",
    "Audience": "EmployeeLeaveSystemClient",
    "ExpiryInMinutes": 60
  }
}
```

### 2e. Add auto‑migration on startup

In `Program.cs`, add this before `app.Run()`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

### 2f. Commit and push

```bash
git add .
git commit -m "Switch to PostgreSQL for Render deployment"
git push origin main
```

---

## 3. Backend — Render Web Service

### 3a. Create a Render account

- Go to [https://dashboard.render.com](https://dashboard.render.com)
- Sign up with GitHub (free, no credit card required)

### 3b. Create a PostgreSQL database

1. Click **New +** → **PostgreSQL**
2. Fill in:
   - **Name**: `employee-leave-db`
   - **Database**: `EmployeeLeaveDb`
   - **User**: `employee_leave_user`
   - **Region**: `Frankfurt (EU Central)` or any near you
   - **Plan**: **Free**
3. Click **Create Database**
4. Copy the **Internal Connection String**

### 3c. Create a Web Service

1. Click **New +** → **Web Service**
2. Connect GitHub → select `Insane-11/EmployeeLeaveSystem`
3. Fill in:
   - **Name**: `employee-leave-system-api`
   - **Region**: same as database
   - **Branch**: `main`
   - **Root Directory**: `EmployeeLeaveSystem.Api`
   - **Runtime**: `.NET 8`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `./out/EmployeeLeaveSystem.Api`
   - **Plan**: **Free**
4. Click **Advanced** and add these environment variables:

| Variable | Value |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | Internal Connection String from step 3b |
| `Jwt__Key` | Random 32+ character string |
| `Jwt__Issuer` | `EmployeeLeaveSystem` |
| `Jwt__Audience` | `EmployeeLeaveSystemClient` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

5. Click **Create Web Service**

The service auto‑runs migrations on startup (from step 2e).

### 3d. Note your Render URL

Your API will be at: `https://employeeleavesystem-w4mo.onrender.com`  
(The subdomain is based on the service name you chose.)

---

## 4. Frontend — Vercel Deployment

### 4a. Configure `environment.prod.ts`

```typescript
export const environment = {
  production: true,
  apiUrl: ''
};
```

> `apiUrl: ''` means API calls go to the same origin. Vercel rewrites `/api/*` to the Render backend — no CORS issues.

### 4b. Update `vercel.json`

Your `employee-leave-ui/vercel.json` proxies API requests to Render:

```json
{
  "buildCommand": "ng build --configuration production",
  "outputDirectory": "dist/employee-leave-ui/browser",
  "rewrites": [
    {
      "source": "/api/(.*)",
      "destination": "https://employeeleavesystem-w4mo.onrender.com/api/$1"
    },
    {
      "source": "/(.*)",
      "destination": "/index.html"
    }
  ]
}
```

> **If your Render URL changes**, update the `destination` here.

### 4c. Deploy to Vercel

1. Go to [https://vercel.com](https://vercel.com)
2. Sign up with GitHub (free)
3. Click **Add New** → **Project**
4. Import `Insane-11/EmployeeLeaveSystem`
5. Set:
   - **Framework Preset**: `Angular`
   - **Root Directory**: `employee-leave-ui`
   - **Build Command**: `ng build --configuration production`
   - **Output Directory**: `dist/employee-leave-ui/browser`
6. Click **Deploy**

### 4d. Get your live URL

After deployment, Vercel gives you a URL like:
`https://employee-leave-system-ten.vercel.app`

You can rename it in Vercel Dashboard → **Settings** → **Domains**.

### 4e. Vercel URL changes on redeploy?

- **Production** deployments keep the same URL (`employee-leave-system.vercel.app`).
- **Preview** deployments (from branches/PRs) get unique, ephemeral URLs.
- If you redeploy the **same branch** (main), the production URL stays fixed.

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `The signature key was not found` | Regenerate the JWT secret in Render env vars and redeploy |
| `Cannot resolve host` on Render | Use the **Internal** connection string (not External) |
| CORS errors | Update `Program.cs` CORS to include your Vercel domain |
| `ng build` fails on Vercel | Verify `angular.json` uses `@angular/build:application` |

---

## Cost Summary

| Service | Cost | Limits |
|---------|------|--------|
| Render Web Service | $0/mo | 512 MB RAM, 1 CPU, spins down after 15 min idle |
| Render PostgreSQL | $0/mo | 1 GB storage |
| Vercel | $0/mo | 100 GB bandwidth, 6000 build minutes |
| GitHub | $0/mo | Unlimited public repos |

---

## Verifying the Deployment

1. Open `https://employee-leave-system-ten.vercel.app`
2. Register a new account (defaults to Employee role)
3. Login and submit a leave request
4. Login as admin (`admin@admin.com` / `Admin123!`) — auto‑seeded on first backend start
5. Approve/reject the request from the admin dashboard
6. Verify the balance deduction
