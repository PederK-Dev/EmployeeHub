# EmployeeHub

A full-stack employee management application — an ASP.NET Core Web API backed by SQL Server and Entity Framework Core, with a React single-page frontend built on Untitled UI. Built as a portfolio project to demonstrate a realistic, secured, end-to-end CRUD application.

## Features

- **JWT authentication** — login, bearer-token protected API, auto-logout on expiry, seeded admin user
- **Role-based authorization** — `Admin` / `Manager` / `Employee` roles enforced on the API and reflected in the UI
- **Dashboard** — live counts (employees, departments, positions), pending-leave count, leave-status breakdown, and recent hires
- **Departments** — full CRUD with validation and an in-use delete guard
- **Positions** — full CRUD
- **Employees** — full CRUD with department & position assignment
- **Leave requests** — submit, approve/reject (manager+), delete, with status badges
- **Users** — admin-only account management with optional employee linking
- **UX polish** — toast notifications, table search, loading & empty states, delete confirmations, and a light/dark theme toggle

## Tech stack

| Layer     | Technology                                                                 |
| --------- | -------------------------------------------------------------------------- |
| Backend   | ASP.NET Core (.NET 10), Entity Framework Core, SQL Server                  |
| Auth      | JWT bearer tokens, role-based policies, ASP.NET Core `PasswordHasher`      |
| Frontend  | React 19 + TypeScript, Vite, React Router, Untitled UI (Tailwind CSS v4)   |

## Screenshots

> Screenshots live in [`docs/screenshots/`](docs/screenshots/). Capture the running app and drop the
> PNGs in with these names to populate this section.

| Login | Dashboard |
| ----- | --------- |
| ![Login page](docs/screenshots/login.png) | ![Dashboard](docs/screenshots/dashboard.png) |

| Employees | Leave requests |
| --------- | -------------- |
| ![Employees](docs/screenshots/employees.png) | ![Leave requests](docs/screenshots/leave-requests.png) |

## Repository layout

```text
EmployeeHub/
├── backend/EmployeeHub/EmployeeHub.Api/   # ASP.NET Core Web API
│   ├── Controllers/                        # Auth, Dashboard, Departments, Employees, Positions, LeaveRequests, Users, Health
│   ├── Services/                           # Business logic (one service per aggregate)
│   ├── Models/                             # EF entities + enums
│   ├── DTOs/                               # Request/response contracts
│   ├── Data/                               # DbContext, migrations, seeder
│   └── Program.cs                          # DI, auth, authorization policies, CORS, pipeline
└── frontend/                               # React SPA (Vite + Untitled UI)
    └── src/
        ├── pages/                          # login, dashboard, departments, positions, employees, leave-requests, users
        ├── components/                     # layout, guards, data-table, modal shell, toasts
        ├── providers/                      # auth, theme, toast, router
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

All endpoints are under `/api`. Every route except `POST /auth/login` and `/health` requires an
`Authorization: Bearer <token>` header. Some routes require a specific role (below).

| Method             | Route                          | Access          | Description                          |
| ------------------ | ------------------------------ | --------------- | ------------------------------------ |
| `POST`             | `/auth/login`                  | Anonymous       | Authenticate, returns a JWT + user   |
| `GET`              | `/auth/me`                     | Authenticated   | Current user from the token          |
| `GET`              | `/dashboard/stats`             | Authenticated   | Aggregated dashboard metrics         |
| `GET/POST`         | `/departments`                 | Authenticated   | List / create departments            |
| `GET/PUT/DELETE`   | `/departments/{id}`            | Authenticated   | Read / update / delete a department  |
| `GET/POST`         | `/positions`                   | Authenticated   | List / create positions              |
| `GET/PUT/DELETE`   | `/positions/{id}`              | Authenticated   | Read / update / delete a position    |
| `GET/POST`         | `/employees`                   | Authenticated   | List / create employees              |
| `GET/PUT/DELETE`   | `/employees/{id}`              | Authenticated   | Read / update / delete an employee   |
| `GET/POST`         | `/leave-requests`              | Authenticated   | List / submit leave requests         |
| `PUT`              | `/leave-requests/{id}/status`  | Manager / Admin | Approve or reject a request          |
| `DELETE`           | `/leave-requests/{id}`         | Manager / Admin | Delete a leave request               |
| `GET/POST`         | `/users`                       | Admin           | List / create user accounts          |
| `DELETE`           | `/users/{id}`                  | Admin           | Delete a user account                |
| `GET`              | `/health`                      | Anonymous       | Health check                         |

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

> **Security note:** the JWT key and seed password in `appsettings.json` are for local development
> only. Move them to user-secrets or environment variables before deploying anywhere real.

## What I learned

- **Layered API design** — keeping controllers thin and pushing logic into per-aggregate services, with DTOs as the boundary so EF entities never leak to clients.
- **Auth vs. authorization** — issuing and validating JWTs is only half the job; without role-based policies (`[Authorize(Policy = ...)]`) any authenticated user could escalate privileges. Adding authentication *and* authorization, and verifying enforcement with different-role tokens, was a key lesson.
- **EF Core relationships & translation** — modeling FKs with the right delete behavior (`Restrict` / `Cascade` / `SetNull`), unique indexes, and storing enums as strings; plus the gotcha that a projection must be an inline `Select` (or a `static Expression`) or EF can't translate it to SQL.
- **CORS & the dev HTTPS pipeline** — debugging a real cross-origin failure where HTTPS redirection was 307-redirecting the browser's preflight `OPTIONS`, which browsers reject.
- **React SPA fundamentals** — auth context, protected/role-guarded routes, a typed `fetch` client with token attachment and auto-logout on 401, and reusable UI (a generic data table, modal shell, toast system).
- **Building on a design system** — composing screens from an existing component library (Untitled UI / React Aria) and its theming, accessibility, and conventions.

## Roadmap

Planned enhancements beyond the current MVP:

- **Auth:** self-service registration, forgot-password flow, and email verification
- **Dashboard:** richer charts (headcount over time, per-department breakdowns) and report export (CSV/PDF)
- **Employees:** dedicated detail pages and pagination/filtering for large datasets
- **Platform:** refresh tokens, audit logging, and containerized deployment (Docker) with CI

## License

This project is for portfolio/demonstration purposes.
