# mytodo — Backend API

A production-minded REST API built with .NET 10 and ASP.NET Core Minimal API. Designed to be clean, secure, and easy to extend.

---

## Table of Contents

- [Tech Stack & Why](#tech-stack--why)
- [Directory Structure](#directory-structure)
- [Architecture Decisions](#architecture-decisions)
- [Data Model](#data-model)
- [Authentication & Authorization](#authentication--authorization)
- [API Reference](#api-reference)
- [Production Features](#production-features)
- [Testing](#testing)
- [Logging](#logging)
- [Tradeoffs & Future Improvements](#tradeoffs--future-improvements)
- [Environment Variables](#environment-variables)

---

## Tech Stack & Why

| Technology | Decision |
|------------|----------|
| **.NET 10** | Latest stable release. Microsoft officially recommends it for all new projects. |
| **Entity Framework Core** | Standard ORM for .NET. Allows database interactions through C# code without writing raw SQL. Models changes via migrations. |
| **SQLite** | Chosen per project requirements. Zero infrastructure, file-based, perfect for an MVP. See [Tradeoffs](#tradeoffs--future-improvements) for production upgrade path. |
| **JWT (JSON Web Tokens)** | Stateless authentication. Tokens are signed and verified server-side — no session storage needed. Scales horizontally without shared state. |
| **BCrypt** | Industry standard for password hashing. One-way hash — plain text passwords are never stored. |
| **xUnit + Moq + FluentAssertions** | Standard .NET testing stack. xUnit for test runner, Moq for mocking dependencies, FluentAssertions for readable assertions. |

---

## Directory Structure

```
api/
  Data/
    AppDbContext.cs         → EF Core database context — registers all models as tables
  Middleware/
    ErrorHandlingMiddleware.cs → Global error handler — catches unhandled exceptions and returns clean JSON responses
  Migrations/               → Auto-generated EF Core migration files — version history of the database schema
  Models/                   → Database entity definitions — each file maps to a database table
  Repositories/             → Data access layer — all database queries live here
  Routes/                   → HTTP route handlers — thin layer that receives requests and calls services
  Services/                 → Business logic layer — all application logic lives here
  Types/
    Requests.cs             → Incoming request shapes (DTOs)
    Responses.cs            → Outgoing response shapes (DTOs)
  appsettings.json          → App configuration (JWT settings, CORS origins)
  Dockerfile                → Docker container definition for Railway deployment
  Program.cs                → Application entry point — wires up all services, middleware, and routes
```

---

## Architecture Decisions

### Minimal API over MVC Controllers

.NET offers two approaches for building HTTP APIs: traditional MVC Controllers and the newer Minimal API pattern. This project uses **Minimal API** for several reasons:

- Microsoft officially recommends it for new projects: *"Minimal APIs are the recommended approach for building fast HTTP APIs with ASP.NET Core"*
- Less boilerplate — no need for controller classes, attribute routing, or action methods
- Slightly better performance due to fewer abstraction layers
- Appropriate for an API of this scope — MVC controllers add structure that pays off in very large codebases but adds ceremony here

For a larger application with dozens of endpoints and complex routing requirements, MVC controllers would be the better choice.

### Route → Service → Repository Pattern

Each layer has a single responsibility:

```
Route Handler    → HTTP concerns only (parse request, call service, return response)
Service          → Business logic (validation, authorization checks, orchestration)
Repository       → Data access (all database queries)
```

This separation means:
- Routes stay thin and readable
- Business logic is testable without HTTP context
- Database queries are centralized and reusable
- Each layer can change independently

### Code-First Database with EF Core

Rather than writing SQL to define the schema, models are defined as C# classes and EF Core generates the SQL. Benefits:
- Schema changes are version controlled as migration files
- No raw SQL scattered throughout the codebase
- Switching databases (e.g. SQLite → PostgreSQL) requires changing one line

### Interface for TokenService

`TokenService` implements `ITokenService` rather than being used directly. This allows the token generation to be mocked in unit tests without needing a real JWT configuration.

---

## Data Model

```
User
  id (UUID), name, email, passwordHash, avatarUrl, createdAt

Project
  id (UUID), title, description, emoji, coverImageUrl, createdAt, updatedAt
  ownerId → User

ProjectMember                    ← join table for project sharing
  id (UUID), role, createdAt
  projectId → Project
  userId → User

TodoTask
  id (UUID), title, description, priority, status, type
  dueDate, completedAt, createdAt, updatedAt
  projectId → Project
  createdById → User
  completedById → User (nullable)

Comment                          ← polymorphic — belongs to either a task OR a project
  id (UUID), body, createdAt
  authorId → User
  taskId → TodoTask (nullable)
  projectId → Project (nullable)
```

### Key Design Decisions

**Polymorphic Comments** — A single Comment table handles both project-level and task-level comments via nullable foreign keys (`taskId` and `projectId`). Exactly one will be set at a time. This avoids needing two separate comment tables.

**CompletedById on Tasks** — Tracks which user marked a task complete, not just when it was completed. This is important for collaborative projects where multiple people share tasks.

**ProjectMember join table** — Enables many-to-many between users and projects. When a project is shared, a `ProjectMember` record is created. The owner is tracked on the `Project` itself (`ownerId`) rather than in the join table.

---

## Authentication & Authorization

### Authentication

JWT-based stateless authentication:

1. User registers or logs in → server validates credentials
2. Server generates a signed JWT containing `userId` and `email` as claims
3. Token is returned to the client and stored in `localStorage`
4. Every subsequent request includes the token in the `Authorization: Bearer` header
5. JWT middleware validates the token signature on every protected request

Tokens expire after **7 days**. In production, the signing secret is overridden by a Railway environment variable. The value in `appsettings.json` is a development placeholder only."

### Authorization

Two levels of authorization are enforced:

**Route level** — `.RequireAuthorization()` on every protected endpoint. Unauthenticated requests are rejected with 401 before reaching the handler.

**Resource level** — Inside service methods, ownership and membership are verified before any operation:

| Operation | Who can perform it |
|-----------|-------------------|
| View project | Owner or any member |
| Edit/Delete project | Owner only |
| Add/remove members | Owner only |
| View tasks | Owner or any member |
| Edit/Delete task | Task creator only |
| Complete task | Owner or any member |
| Add comment | Owner or any member |
| Edit/Delete comment | Comment author only |

### Password Security

Passwords are hashed with BCrypt before storage. BCrypt is intentionally slow and includes a salt, making it resistant to brute force and rainbow table attacks. Plain text passwords are never stored or logged.

---

## API Reference

All protected endpoints require `Authorization: Bearer <token>` header.

### Auth
```
POST /auth/register    → Register a new account
POST /auth/login       → Login and receive a JWT token
```

### Projects
```
GET    /api/projects                           → List all projects (owner or member)
POST   /api/projects                           → Create a new project
GET    /api/projects/:id                       → Get project detail with task counts
PATCH  /api/projects/:id                       → Update project
DELETE /api/projects/:id                       → Delete project and all associated data
```

### Tasks
```
GET    /api/projects/:id/tasks                 → List tasks with optional filters
POST   /api/projects/:id/tasks                 → Create a task
GET    /api/tasks/:id                          → Get task detail
PATCH  /api/tasks/:id                          → Update task
DELETE /api/tasks/:id                          → Delete task (creator only)
```

### Comments
```
GET    /api/projects/:id/comments              → List project comments
POST   /api/projects/:id/comments              → Add project comment
GET    /api/tasks/:id/comments                 → List task comments
POST   /api/tasks/:id/comments                 → Add task comment
PATCH  /api/comments/:id                       → Update comment (author only)
DELETE /api/comments/:id                       → Delete comment (author only)
```

### Members
```
GET    /api/projects/:id/members               → List project members
POST   /api/projects/:id/members               → Add a member (owner only)
DELETE /api/projects/:id/members/:userId       → Remove a member (owner only)
```

### Users
```
GET    /api/users/search?name=                 → Search users by name (for sharing)
```

### Task Filtering & Sorting

The GET tasks endpoint supports query parameters:
```
?status=Todo&status=In Progress    → Filter by one or more statuses
?priority=High&priority=Urgent     → Filter by one or more priorities
?createdById=<uuid>                → Filter by task creator
?sortBy=duedate|priority|status|createdby
?sortOrder=asc|desc
```

---

## Production Features

### Global Error Handling
`ErrorHandlingMiddleware` catches all unhandled exceptions and returns a consistent JSON error shape. Stack traces are never exposed to clients — full details are logged server-side only.

```json
{ "error": "An unexpected error occurred" }
```

Known error types are mapped to appropriate HTTP status codes:
- `ArgumentException` → 400 Bad Request
- `UnauthorizedAccessException` → 401 Unauthorized
- `KeyNotFoundException` → 404 Not Found
- Everything else → 500 Internal Server Error

### Rate Limiting
Auth endpoints (`/auth/register`, `/auth/login`) are rate limited to **10 requests per minute** per client using .NET's built-in rate limiter. Exceeding the limit returns HTTP 429. This prevents brute force password attacks.

### CORS
Configured to only allow requests from the deployed frontend origin. The allowed origin is set via environment variable so it can be changed per environment without a code change.

### Auto Migrations
On startup, the app automatically applies any pending database migrations via `db.Database.Migrate()`. This ensures the database schema is always up to date after a deployment without manual intervention.

---

## Testing

Unit tests covering the most critical paths — authentication, authorization, and core business logic.

```bash
cd api.tests
dotnet test
```

### Test Coverage

| Service | Tests |
|---------|-------|
| AuthService | Register/login validation, duplicate emails, wrong passwords |
| TokenService | Token generation, JWT format, uniqueness |
| ProjectService | CRUD, access control, member management, comment and task creation |
| TaskService | CRUD, creator-only edit/delete, member status updates |
| CommentService | Update/delete author enforcement |

### Testing Approach

**Unit tests** using an **in-memory SQLite database** — no mocking of the database layer. This gives confidence that the actual queries work correctly while keeping tests fast and isolated. Each test gets a fresh database instance via `Guid.NewGuid()` as the database name.

Dependencies like `TokenService` and `ILogger` are mocked using Moq.

**Note on FluentAssertions:** The project uses FluentAssertions for readable test assertions. This library requires a commercial license for commercial use — for an MVP evaluation this is acceptable, but a production codebase would either purchase a license or switch to a different assertions library.

---

## Logging

Structured logging using .NET's built-in `ILogger`. Log levels are used intentionally:

- **`LogInformation`** — normal flow events worth tracking (user registered, project created, task deleted)
- **`LogWarning`** — expected failure cases worth investigating (failed login attempt, access denied, resource not found)
- **`LogError`** — unexpected system failures (caught by error handling middleware)

Every service method logs on entry with relevant IDs so a complete request flow can be traced through the logs. Sensitive data (passwords, emails) is never logged.

---

## Tradeoffs & Future Improvements

### SQLite → PostgreSQL
SQLite was chosen per the project requirements and is appropriate for this MVP. In production, SQLite has a critical limitation: the database file is wiped on every container restart. Switching to PostgreSQL would require:
1. Adding the `Npgsql.EntityFrameworkCore.PostgreSQL` package
2. Changing the EF Core provider in `Program.cs`
3. Updating the connection string

### JWT → JWT + Refresh Tokens
Current tokens expire after 7 days. A production system would use short-lived access tokens (15 minutes) paired with long-lived refresh tokens, reducing the window of exposure if a token is compromised.

### Social Auth (Google/GitHub Login)
Most personal apps benefit from OAuth login. Users don't want to manage another password. This would be implemented using ASP.NET Core's OAuth middleware.

### Image Uploads (S3/Cloudflare R2)
Both `User.AvatarUrl` and `Project.CoverImageUrl` fields exist on the models but are not currently exposed in the UI. Implementing this properly requires:
- A file upload endpoint in the API
- An S3 or Cloudflare R2 bucket to store images
- Returning the public URL to store in the database

The data model already supports this — no migration needed.

### Task Types
`TodoTask.Type` exists on the model but is not currently used in the UI. The planned implementation would allow users to categorize tasks (e.g. Errand, Finance, Personal, Health) for better organization and filtering.

### Viewer Role for Project Members
Currently project members have full edit access. A future improvement would add a `Viewer` role (read-only access) to the `ProjectMember` model, useful for sharing progress with stakeholders.

### Full Name → First + Last Name
`User.Name` is currently a single string. Splitting into `FirstName` and `LastName` would allow more flexible display options (e.g. showing just first names in task rows).

### Pagination
Task and comment lists currently return all records. For projects with many tasks, cursor-based pagination would be needed. EF Core's `.Skip()` and `.Take()` make this straightforward to add.

### FluentValidation
Input validation is currently done with manual `if` checks in service methods. A library like FluentValidation would provide a cleaner, declarative way to define validation rules in a dedicated class per request type.

---

## Environment Variables

| Variable | Description | Required |
|----------|-------------|----------|
| `Jwt__SecretKey` | Secret key for signing JWT tokens. Must be at least 32 characters. | Yes |
| `Jwt__Issuer` | JWT issuer identifier. Must match across all environments. | Yes |
| `Jwt__Audience` | JWT audience identifier. Must match across all environments. | Yes |
| `AllowedOrigins` | Comma-separated list of allowed frontend origins for CORS. | Yes |
| `DATABASE_URL` | SQLite connection string. Defaults to `Data Source=mytodo.db` if not set. | No |
| `PORT` | Port for the server to listen on. Set automatically by Railway. | No |

Note: In .NET, nested config keys use `__` as the separator in environment variables (e.g. `Jwt__SecretKey` maps to `Jwt:SecretKey` in `appsettings.json`).