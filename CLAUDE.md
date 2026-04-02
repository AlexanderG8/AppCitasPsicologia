# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**AppCitasPsicologia** is an ASP.NET Core 10 MVC web application for managing psychology appointments. It uses Dapper (not Entity Framework) for data access against a SQL Server database (`BD_PSICOLOGIA`).

## Common Commands

```bash
# Build .NET (also auto-runs Tailwind CSS build via pre-build target)
dotnet build

# Run (development)
dotnet run

# Run with HTTPS
dotnet run --launch-profile https

# Build Tailwind CSS manually (outputs to wwwroot/css/site.css)
npm run build:css

# Watch mode for CSS during active development
npm run watch:css
```

- HTTP: http://localhost:5283
- HTTPS: https://localhost:7138

## Database Setup

The database schema is in `../ProyectoPsicologia.sql` (parent directory). Run it against SQL Server Express to initialize `BD_PSICOLOGIA`.

Connection string is in `appsettings.json` (excluded from git). Use `appsettings.Development.json` as a reference for the format:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=<server>\\SQLEXPRESS;Database=BD_PSICOLOGIA;Integrated Security=True;Trust Server Certificate=True"
}
```

## Architecture

**Stack:** ASP.NET Core 10 MVC + Dapper + SQL Server + Tailwind CSS v3

**Layer pattern:**

```
Controllers → Repositories (interface + impl) → Dapper → SQL Server
```

- No Entity Framework — all queries are raw SQL via Dapper.
- No Service layer yet — controllers inject repositories directly.
- All database operations are async (`Task<T>`).

**Dependency injection** is registered in `Program.cs` as Transient:
```csharp
builder.Services.AddTransient<IRepositorioRoles, RepositorioRoles>();
```

Follow this pattern when adding new repositories.

## Key Database Tables

| Table | Purpose |
|---|---|
| `ROLES` | User roles |
| `OPCIONES` | Menu options per role |
| `USUARIOS` | Users (admin, psychologist, client) |
| `SUSCRIPCIONES` | Subscription plans |
| `DETALLESUSCRIPCIONES` | Active subscriptions per admin |
| `SERVICIOS` | Services offered |
| `SERVICIOSPSICOLOGOS` | Psychologist ↔ service assignments |
| `CITAS` | Appointments |

## Code Conventions

- **Namespace root:** `AppCitasPsicologia`
- **Repositories** go in `Repositorys/` — interface + implementation in the same folder.
- **Models** go in `Models/<Entity>/` subfolder.
- **Custom validation attributes** go in `Models/Validations/`.
- Pagination uses `PaginacionViewModel` (input) and `PaginacionRespuesta<T>` (output) from `Models/Paginacion/`.
- The `PrimeraLetraMayusculaAttribute` custom validator enforces capitalized first letter — reuse it for name fields.

## Views & Frontend

- Layout: `Views/Shared/_Layout.cshtml` — fixed sidebar (slate-900) + main content area.
- Reusable pagination partial: `Views/Shared/_Paginacion.cshtml`.
- UI uses **Tailwind CSS v3** (utility-first). No Bootstrap. jQuery and jQuery UI are in `wwwroot/lib/`.
- CSS source: `wwwroot/css/app.css` (Tailwind directives) → compiled to `wwwroot/css/site.css` (gitignored).
- Tailwind config: `tailwind.config.js` — content paths point to `Views/**/*.cshtml`.
- Icons are inline SVGs (Heroicons style) — no icon font dependency.
- Design system: sidebar `slate-900`, accent `indigo-600`, backgrounds `gray-50`/`white`, borders `slate-200`.
- App language is Spanish (`lang="es"`).

## Implemented Features

Only **Roles CRUD** is complete. The remaining entities (Users, Appointments, Services, Subscriptions) have database tables but no controllers or views yet.
