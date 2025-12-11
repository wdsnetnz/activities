# Copilot Instructions for Activities Codebase

## Architecture Overview

This is a full-stack **CQRS + Domain-Driven Design** application with a **.NET 9 backend** and **React/TypeScript frontend**.

### Project Structure (4-Layer .NET Backend)

- **Domain** (`Domain/`): Pure entities (no dependencies). Contains `Activity` model with Id, Title, Date, Description, Category, City, Venue, Latitude/Longitude.
- **Persistence** (`Persistence/`): Entity Framework Core DbContext (`AppDbContext`), migrations, and `DbInitializer` for seeding.
- **Application** (`Application/`): CQRS command/query handlers organized in `Features/Commands/` and `Features/Queries/`. Uses MediatR for orchestration and AutoMapper for mapping.
- **API** (`API/`): ASP.NET Core controllers inheriting from `BaseApiController` (which provides lazy-loaded `Mediator` property). Registered in `Program.cs` with dependency injection.

### Frontend
- **Client** (`client/`): React 19 + TypeScript + Vite with Material-UI components. Communicates via Axios to `https://localhost:5001/api/activities`.

## Key Patterns & Conventions

### CQRS Implementation
Each operation (Create, Edit, Delete, Get) is a nested class structure: `Command`/`Query` (request DTO) + `Handler` (implements `IRequestHandler<T>`).

**Example - CreateActivity** (`Application/Features/Commands/CreateActivity.cs`):
```csharp
public class CreateActivity
{
    public class Command : IRequest<string> { public required Activity Activity { get; set; } }
    public class Handler(AppDbContext context) : IRequestHandler<Command, string>
    {
        public async Task<string> Handle(Command request, CancellationToken cancellationToken)
        { /* business logic */ }
    }
}
```

Handlers use **constructor injection** with nullable-enabled implicit usings.

### Data Access & EF Core
- `AppDbContext` uses SQLite (`DefaultConnection` in `appsettings.json`).
- Auto-migrations run at startup in `Program.cs` via `context.Database.MigrateAsync()`.
- Database seeding handled by `DbInitializer.SeedData()` (checks if data exists before inserting).

### Controller Pattern
Controllers inherit from `BaseApiController` to access protected `Mediator` property (lazy-loaded from DI). Never instantiate handlers directly; always dispatch via MediatR:
```csharp
var result = await Mediator.Send(new GetActivityList.Query());
```

### AutoMapper Configuration
- Minimal profile in `Application/Core/MappingProfiles.cs` (currently only maps `Activity` to `Activity`).
- Only used in `EditActivity.Handler` for DTO-to-entity mapping.
- Register with `builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly)`.

### CORS & Frontend Integration
- CORS configured for `http://localhost:3000` and `https://localhost:3000` in `Program.cs`.
- Frontend uses absolute HTTPS URLs (`https://localhost:5001`); ensure SSL certificates match during local development.

## Build & Run Commands

### Backend (.NET 9)
```powershell
# Restore & build
dotnet build

# Run API (starts on https://localhost:5001, http://localhost:5142)
dotnet run --project API

# Run migrations only
dotnet ef database update --project Persistence --startup-project API

# Add new migration
dotnet ef migrations add MigrationName --project Persistence --startup-project API
```

### Frontend (Node 18+)
```powershell
# Navigate to client/
cd client

# Install dependencies
npm install

# Dev server (Vite HMR on http://localhost:3000)
npm run dev

# Build for production
npm run build

# Lint
npm run lint
```

## Testing & Validation

- No automated tests currently in project; consider adding xUnit for handlers in `Application.Tests/`.
- Use `API/API.http` file for manual endpoint testing (REST Client extension compatible).
- Verify migrations: `dotnet ef migrations list --project Persistence --startup-project API`.

## Common Tasks

### Add a New Feature
1. Add domain model changes to `Domain/Activity.cs`.
2. Create migration: `dotnet ef migrations add FeatureName --project Persistence --startup-project API`.
3. Create handler in `Application/Features/Commands/` or `Queries/` (follow CQRS naming).
4. Add controller endpoint in `API/Controllers/ActivitiesController.cs`.
5. Update AutoMapper profile if DTOs differ from entities.
6. Test via `API.http` or frontend component.

### Modify Database Schema
- Only edit `Domain/Activity.cs`; migrations are auto-generated.
- Run `dotnet ef migrations add DescriptiveName` after changes.
- Migrations apply automatically on app startup.

### Frontend API Integration
- All backend calls go through `axios` to `https://localhost:5001/api/activities`.
- Define Activity type in `client/src/lib/types/` (not yet created; suggest for new features).
- Use Material-UI components for consistent UI.

## Project-Specific Notes

- **Nullable Reference Types** enabled (`<Nullable>enable</Nullable>`) — use `required` keyword and nullable-aware patterns.
- **Implicit Usings** enabled — no need for `using System;` etc. in most files.
- **Target Framework**: .NET 9.0 (latest LTS-adjacent version).
- No logging framework configured beyond default ASP.NET Core; add Serilog if needed.
- Database is SQLite in-process; seed data re-runs only if empty (safe for dev, replace in production).
