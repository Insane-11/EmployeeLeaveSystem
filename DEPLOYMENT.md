# 🚀 Free Deployment Guide

Deploy the **Employee Leave Management System** online at **zero cost** using **Render** (backend + PostgreSQL) and **Vercel** (frontend).

---

## Table of Contents

1. [Prepare GitHub Repository](#step-1-prepare-github-repository)
2. [Backend — PostgreSQL Migration](#step-2-backend--postgresql-migration)
3. [Backend — Render Web Service](#step-3-backend--render-web-service)
4. [Frontend — Vercel Deployment](#step-4-frontend--vercel-deployment)
5. [Testing the Live App](#step-5-testing-the-live-app)

---

## Step 1: Prepare GitHub Repository

Your code is already committed and pushed. Make sure the repo is public so Render and Vercel can access it:

```bash
git remote -v
# Should show: origin  https://github.com/Insane-11/EmployeeLeaveSystem.git
```

---

## Step 2: Backend — PostgreSQL Migration

Render's free tier doesn't support SQL Server. You'll switch the backend to **PostgreSQL** using the `Npgsql` EF Core provider.

### 2a. Add the Npgsql package

```bash
cd EmployeeLeaveSystem.Api
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
```

### 2b. Update `Program.cs`

Replace the `UseSqlServer` line with `UseNpgsql`:

```csharp
// Program.cs — find this line:
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

// Replace it with:
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
```

### 2c. Update `appsettings.json`

Change the connection string placeholder for PostgreSQL (you'll fill in the real values on Render):

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

### 2d. Generate EF migrations for PostgreSQL

Delete the existing SQL Server migrations and create fresh ones:

```bash
# Remove old migrations
rm -rf Migrations

# Create new migration
dotnet ef migrations add InitialCreate
```

### 2e. Commit and push

```bash
git add .
git commit -m "Switch to PostgreSQL for Render deployment"
git push origin main
```

---

## Step 3: Backend — Render Web Service

### 3a. Create a Render account

- Go to [https://dashboard.render.com](https://dashboard.render.com)
- Sign up with **GitHub** (free, no credit card required)

### 3b. Create a PostgreSQL database

1. Click **New +** → **PostgreSQL**
2. Fill in:
   - **Name**: `employee-leave-db`
   - **Database**: `EmployeeLeaveDb`
   - **User**: `employee_leave_user`
   - **Region**: `Frankfurt (EU Central)` or any near you
   - **Plan**: **Free**
3. Click **Create Database**
4. Wait a minute, then copy the **Internal Connection String** (looks like `postgres://user:pass@host:5432/EmployeeLeaveDb`)

> 💡 **Keep this tab open** — you'll need the connection string in the next step.

### 3c. Create a Web Service

1. Click **New +** → **Web Service**
2. **Connect GitHub repository** → select `Insane-11/EmployeeLeaveSystem`
3. Fill in:
   - **Name**: `employee-leave-system-api`
   - **Region**: same as your database
   - **Branch**: `main`
   - **Root Directory**: `EmployeeLeaveSystem.Api`
   - **Runtime**: `.NET 8`
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `./out/EmployeeLeaveSystem.Api`
   - **Plan**: **Free**
4. Click **Advanced** and add these environment variables:

| Variable | Value |
|----------|-------|
| `ConnectionStrings__DefaultConnection` | Paste the **Internal Connection String** from step 3b |
| `Jwt__Key` | A random 32+ character string (e.g. `a9f8d7e6c5b4a3f2e1d0c9b8a7f6e5d4`) |
| `Jwt__Issuer` | `EmployeeLeaveSystem` |
| `Jwt__Audience` | `EmployeeLeaveSystemClient` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

> ⚠️ **Note**: Render uses double underscores (`__`) to represent nested config keys (e.g. `ConnectionStrings__DefaultConnection` maps to `ConnectionStrings:DefaultConnection`).

5. Click **Create Web Service**

### 3d. Run database migrations

Once the service is **Live** (green status):

1. Go to your service dashboard → **Shell** tab
2. Run these commands:

```bash
cd EmployeeLeaveSystem.Api
dotnet ef database update
```

> If the Shell tab isn't available, create a one‑time endpoint in `Program.cs` that runs migrations on startup (add this before `app.Run()`):

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

3. After migrations succeed, the backend is ready.

### 3e. Note your Render URL

Your API will be at: `https://employee-leave-system-api.onrender.com`  
(You can set a custom name — whatever you chose in step 3c.)

---

## Step 4: Frontend — Vercel Deployment

### 4a. Prepare environment files

First, create the Angular environment files (already done in this repo — skip if `src/environments/` exists):

```typescript
// src/environments/environment.ts (development)
export const environment = {
  production: false,
  apiUrl: ''
};
```

```typescript
// src/environments/environment.prod.ts (production)
export const environment = {
  production: true,
  apiUrl: 'https://employee-leave-system-api.onrender.com'
};
```

Update `angular.json` to swap files during production builds (already done if the `configurations.production` section has `fileReplacements`):

```json
"configurations": {
  "production": {
    "fileReplacements": [
      {
        "replace": "src/environments/environment.ts",
        "with": "src/environments/environment.prod.ts"
      }
    ],
    "budgets": [...]
  }
}
```

### 4b. Update Angular services

Update each service to prepend the API URL (already done in this repo — verify `auth.service.ts`, `leave-request.service.ts`, `user.service.ts` use `environment.apiUrl`):

```typescript
import { environment } from '../../../environments/environment';

// In the service:
private readonly baseUrl = `${environment.apiUrl}/api/auth`;
```

### 4c. Create `vercel.json`

A `vercel.json` in the `employee-leave-ui/` root tells Vercel how to serve your Angular app and proxy API calls (already done in this repo):

```json
{
  "buildCommand": "ng build --prod",
  "outputDirectory": "dist/employee-leave-ui",
  "rewrites": [
    { "source": "/api/(.*)", "destination": "https://employee-leave-system-api.onrender.com/api/$1" }
  ]
}
```

> ⚠️ **Security note**: The rewrite above exposes your Render URL. For a demo/resume project this is fine. In production, use a custom domain or server‑side proxy.

### 4d. Deploy to Vercel

1. Go to [https://vercel.com](https://vercel.com)
2. Sign up with **GitHub** (free, no credit card)
3. Click **Add New** → **Project**
4. Import `Insane-11/EmployeeLeaveSystem`
5. Set:
   - **Framework Preset**: `Angular`
   - **Root Directory**: `employee-leave-ui` (click **Edit** and select it)
   - **Build Command**: `ng build --prod` (Vercel detects this automatically)
   - **Output Directory**: `dist/employee-leave-ui`
6. Add an environment variable:
   - `API_BASE_URL` = `https://employee-leave-system-api.onrender.com`
7. Click **Deploy**

### 4e. Get your live URL

After deployment, Vercel gives you a URL like:  
`https://employee-leave-system.vercel.app`

You can **rename** the project in Vercel dashboard → **Settings** → **Domain** to get a cleaner name.

---

## Step 5: Testing the Live App

1. Open your Vercel URL: `https://employee-leave-system.vercel.app`
2. **Register** a new account (role defaults to **Employee**)
3. **Login** and submit a leave request
4. Verify you see the toast notification ("Leave request submitted") and the request appears in "My Requests"
5. Try submitting a past date — you should see the validation error: "Start date cannot be in the past."
6. Login as a different user to test manager/admin roles (you can change roles via direct DB update or the admin panel)

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `The signature key was not found` | Regenerate the JWT secret in Render env vars and redeploy |
| `Cannot resolve host` on Render | Make sure the PostgreSQL connection string uses the **Internal** connection string (not External) |
| `Failed to bind to port` on Render | Render sets `PORT` env var automatically. Your `Program.cs` should respect it — Kestrel does this by default in .NET 8 |
| CORS errors in browser | Update `Program.cs` CORS policy to include your Vercel domain: `policy.WithOrigins("https://employee-leave-system.vercel.app")` |
| `ng build` fails on Vercel | Make sure `angular.json` uses `@angular/build:application` and `package.json` includes all dependencies |

---

## Cost Summary

| Service | Cost |
|---------|------|
| Render Web Service (Free) | $0 / month — 512 MB RAM, 1 CPU, sleeps after 15 min idle |
| Render PostgreSQL (Free) | $0 / month — 1 GB storage |
| Vercel (Free) | $0 / month — 100 GB bandwidth, 6000 build minutes |
| GitHub (Free) | $0 / month — unlimited public repos |

> ⚠️ **Render free tier caveat**: The web service **spins down after 15 minutes of inactivity**. The first request after idle takes 30–60 seconds to wake up. For a resume demo this is acceptable.

---

Your app is now live on the internet! 🎉 Add the Vercel URL to your resume under "Projects" or "Links".
