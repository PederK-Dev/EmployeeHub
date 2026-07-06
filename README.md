# EmployeeHub

A full-stack employee management application: an ASP.NET Core Web API backed by SQL Server and Entity Framework Core, with a React single-page frontend built on Untitled UI.

## Features

- **JWT authentication** — sign in, bearer-token protected API, auto-logout on expiry
- **Dashboard** — organization overview (department, employee, and position counts + recent hires)
- **Department management** — full CRUD
- **Employee management** — full CRUD with department/position assignment
- Backing domain models for **Positions**, **Leave requests**, and **Users**

## Tech stack

| Layer     | Technology                                                              |
| --------- | ----------------------------------------------------------------------- |
| Backend   | ASP.NET Core (.NET 10), Entity Framework Core, SQL Server               |
| Auth      | JWT bearer tokens, ASP.NET Core `PasswordHasher`                        |
| Frontend  | React 19 + TypeScript, Vite, React Router, Untitled UI (Tailwind CSS v4)|

## Repository layout

```text
EmployeeHub/
├── backend/EmployeeHub/EmployeeHub.Api/   # ASP.NET Core Web API
│   ├── Controllers/                        # Auth, Departments, Employees, Positions, LeaveRequests, Users, Health
│   ├── Services/                           # Business logic (one service per aggregate)
│   ├── Models/                             # EF entities + enums
│   ├── DTOs/                               # Request/response contracts
│   ├── Data/                               # DbContext, migrations, seeder
│   └── Program.cs                          # DI, auth, CORS, pipeline
└── frontend/                               # React SPA (Vite + Untitled UI)
    └── src/
        ├── pages/                          # login, dashboard, departments, employees
        ├── components/                     # layout, protected route, modal shell
        ├── providers/                      # auth context, theme, router
        └── lib/                            # API client + types
```

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 20+ and npm
- SQL Server (the default connection string targets a local `SQLEXPRESS` instance)
- EF Core tools (for running migrations): `dotnet tool install --global dotnet-ef`

## Getting started

The app requires **both** the API and the frontend running at the same time. Use two terminals.

### 1. Backend API

```bash
cd backend/EmployeeHub/EmployeeHub.Api
dotnet run
```

On first run the API automatically applies EF migrations and seeds:

- an **admin user** — `admin@employeehub.local` / `Admin123!`
- a couple of starter departments and positions

The API listens on **<http://localhost:5023>** (see `Properties/launchSettings.json`).

### 2. Frontend

```bash
cd frontend
npm install      # first time only
npm run dev
```

Open the printed URL — **<http://localhost:5173>** — and sign in with the seeded admin credentials.

> The frontend reads the API base URL from `frontend/.env` (`VITE_API_URL`). If Vite starts on a
> different port, add that origin to `Cors:AllowedOrigins` in `appsettings.json` or the browser will
> block API calls.

## API overview

All endpoints are under `/api`. Every route except `POST /auth/login` and `/health` requires a
`Authorization: Bearer <token>` header.

| Method                | Route                          | Description                              |
| --------------------- | ------------------------------ | ---------------------------------------- |
| `POST`                | `/auth/login`                  | Authenticate, returns a JWT + user       |
| `GET`                 | `/auth/me`                     | Current user from the token              |
| `GET/POST`            | `/departments`                 | List / create departments                |
| `GET/PUT/DELETE`      | `/departments/{id}`            | Read / update / delete a department      |
| `GET/POST`            | `/employees`                   | List / create employees                  |
| `GET/PUT/DELETE`      | `/employees/{id}`              | Read / update / delete an employee       |
| `GET/POST`            | `/positions`                   | List / create positions                  |
| `GET/PUT/DELETE`      | `/positions/{id}`              | Read / update / delete a position        |
| `GET/POST`            | `/leave-requests`              | List / create leave requests             |
| `PUT`                 | `/leave-requests/{id}/status`  | Approve / reject a leave request         |
| `GET/POST`            | `/users`                       | List / create users                      |
| `GET`                 | `/health`                      | Health check (open)                      |

## Database migrations

From `backend/EmployeeHub/EmployeeHub.Api`:

```bash
dotnet ef migrations add <Name>   # scaffold a migration after model changes
dotnet ef database update         # apply migrations (also runs automatically on startup)
```

## Configuration

Backend settings live in `backend/EmployeeHub/EmployeeHub.Api/appsettings.json`:

- `ConnectionStrings:DefaultConnection` — SQL Server connection
- `Jwt` — signing key, issuer, audience, token lifetime
- `Cors:AllowedOrigins` — allowed frontend origins
- `Seed` — admin credentials created on first run

> **Note:** the JWT key and seed password in `appsettings.json` are for local development only. Move
> them to user-secrets or environment variables before deploying anywhere real.
