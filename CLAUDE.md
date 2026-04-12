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
| `OPCIONES` | Menu options (Controlador + Accion fields map to MVC routing) |
| `OPCIONESROL` | Role ↔ option assignments (junction table) |
| `USUARIOS` | Users (admin, psychologist, patient) |
| `EMPRESAS` | Companies |
| `SUSCRIPCIONES` | Subscription plans |
| `DETALLESUSCRIPCIONES` | Active subscriptions per company |
| `SERVICIOS` | Services offered per company |
| `SERVICIOSPSICOLOGOS` | Psychologist ↔ service assignments (junction table) |
| `CITAS` | Appointments |

## Code Conventions

- **Namespace root:** `AppCitasPsicologia`
- **Repositories** go in `Repositorys/` — interface + implementation in the same file.
- **Models** go in `Models/<Entity>/` subfolder.
- **ViewComponents** go in `ViewComponents/` — component class + `Views/Shared/Components/<Name>/Default.cshtml`.
- **Custom validation attributes** go in `Models/Validations/`.
- Pagination uses `PaginacionViewModel` (input) and `PaginacionRespuesta<T>` (output) from `Models/Paginacion/`.
- The `PrimeraLetraMayusculaAttribute` custom validator enforces capitalized first letter — reuse it for name fields.
- Junction table pattern (DELETE all + bulk INSERT in transaction) used for `OPCIONESROL` and `SERVICIOSPSICOLOGOS`.
- Soft delete: set `FechaEliminado = GETDATE()`, filter with `FechaEliminado IS NULL`.

## Views & Frontend

- Layout: `Views/Shared/_Layout.cshtml` — fixed sidebar (slate-900) + main content area.
- Reusable pagination partial: `Views/Shared/_Paginacion.cshtml`.
- UI uses **Tailwind CSS v3** (utility-first). No Bootstrap. jQuery and jQuery UI are in `wwwroot/lib/`.
- CSS source: `wwwroot/css/app.css` (Tailwind directives) → compiled to `wwwroot/css/site.css` (gitignored).
- Tailwind config: `tailwind.config.js` — content paths point to `Views/**/*.cshtml`.
- Icons are inline SVGs (Heroicons style) — no icon font dependency.
- Design system: sidebar `slate-900`, accent `indigo-600`, backgrounds `gray-50`/`white`, borders `slate-200`.
- App language is Spanish (`lang="es"`).
- Toast notifications: set `TempData["Toast"] = "mensaje"` in any POST action — the layout renders and auto-dismisses it.
- **Pagination JS**: always include the action name in the URL — use `/Controller/Index?pagina=1&...`, NOT `/Controller/?pagina=1&...` (the default route action is `Login`, not `Index`).

## Services

**`IServicioUsuario`** (`Services/ServicioUsuario.cs`, namespace `ManejoPresupuesto.Services`):
```csharp
int ObtenerUsuarioId();               // reads ClaimTypes.NameIdentifier
Task<int> ObtenerEmpresaIdAsync();    // looks up USUARIOS.EmpresaId
Task<int> ObtenerRolIdAsync();        // looks up USUARIOS.RolId
```
Use this in any controller that needs the logged-in user's context instead of duplicating the lookup.

## Cross-controller redirects (returnUrl pattern)

`UsuariosController` POST actions (`Crear`, `Editar`, `BorrarUsuario`, `RestoreKey`) accept an optional `string returnUrl = null`. Views that delegate their POST to `UsuariosController` pass a hidden `returnUrl` field pointing back to their own Index:

```html
<input type="hidden" name="returnUrl" value="@Url.Action("Index", "Administradores")" />
```

The controller validates with `Url.IsLocalUrl(returnUrl)` before redirecting.

## ViewComponents

Located in `ViewComponents/`, views in `Views/Shared/Components/<Name>/`.

| Component | Invoke | Purpose |
|---|---|---|
| `Navegacion` | `@await Component.InvokeAsync("Navegacion")` | Dynamic sidebar nav — shows only options assigned to the user's role via `OPCIONESROL` |
| `UsuarioInfo` | `@await Component.InvokeAsync("UsuarioInfo")` | Sidebar bottom: avatar + name + role |
| `UsuarioInfo` (header) | `@await Component.InvokeAsync("UsuarioInfo", new { soloHeader = true })` | Header dropdown button: small avatar + name only |

Both components are guarded in `_Layout.cshtml` with `@if (User.Identity?.IsAuthenticated == true)`.

## Implemented Features

| Feature | Controller | Views |
|---|---|---|
| Roles CRUD | `RolesController` | Index, Crear, Editar |
| Roles → Opciones assignment | `RolesController` | OpcionesDeRol |
| Opciones CRUD | `OpcionesController` | Index, Crear, Editar |
| Empresas CRUD | `EmpresasController` | Index, Crear, Editar |
| Empresas → Suscripciones detail | `EmpresasController` | DetalleSuscripcionEmpresa, CrearDetalleSuscripcionEmpresa, EditarDetalleSuscripcionEmpresa |
| Suscripciones CRUD | `SuscripcionesController` | Index, Crear, Editar |
| Servicios CRUD (per company) | `ServiciosController` | Index, Crear, Editar |
| Usuarios CRUD | `UsuariosController` | Index, Crear, Editar |
| Administradores CRUD | `AdministradoresController` (GET) + `UsuariosController` (POST) | Index, Crear, Editar |
| Psicólogos CRUD | `PsicologosController` (GET) + `UsuariosController` (POST) | Index, Crear, Editar |
| Psicólogos → Servicios assignment | `PsicologosController` | ServiciosPsicologo |
| Pacientes CRUD | `PacientesController` (GET) + `UsuariosController` (POST) | Index, Crear, Editar |
| Perfil de usuario | `UsuariosController` | Perfil |
| Auth (login / logout / set password) | `UsuariosController` | Login, EstablecerContrasena, TokenInvalido |
| Dynamic sidebar navigation | `NavegacionViewComponent` | — |
| User info in layout | `UsuarioInfoViewComponent` | — |

## Authentication & Identity

- ASP.NET Core Identity with custom `UsuarioStore` (no EF), `AddIdentityCore<Usuarios>().AddSignInManager()`.
- Cookie auth via `IdentityConstants.ApplicationScheme`, login path `/Usuarios/Login`.
- New users receive an email invitation with a GUID token (24 h expiry) to set their password via `EstablecerContrasena`.
- `RestoreKey` action resends the invitation email for password reset.
- Role codes in DB: `ADM` (Administrador), `PSIC` (Psicólogo), `PACI` (Paciente).
