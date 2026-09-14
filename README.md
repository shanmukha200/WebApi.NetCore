# WebApi.NetCore (.NET 8)

Production-ready ASP.NET Core Web API using **JWT authentication**, **refresh tokens**, **role-based authorization**, and **Dapper** with SQL Server.

## Project Structure

- `Controllers/`
  - `AuthController` (`/api/auth/*`)
  - `UsersController` (`/api/users/*`)
  - `AdminController` (`/api/admin/*`)
- `Services/`
  - `AuthService`
  - `JwtTokenService`
  - `AdminService`
- `Data/`
  - `IDataAccess`, `DataAccess` (Dapper)
  - `Repositories/` (`UserRepository`, `RefreshTokenRepository`)
  - `Database/DatabaseInitializer.cs`
  - `Database/schema.sql`
- `Models/` (`User`, `RefreshToken`, `PaginationResult`)
- `Dtos/` (request/response contracts)
- `Middleware/` (`ErrorHandlingMiddleware`, `AuthenticationMiddleware`)
- `Utilities/` (`PasswordHasher` PBKDF2-SHA256)
- `Constants/` (`AppConstants` roles)

## Prerequisites

- .NET SDK 8.0+
- SQL Server (or LocalDB for development)

## Setup

1. Update `ConnectionStrings:DefaultConnection` in `appsettings*.json`.
2. Set a strong JWT secret (32+ bytes) in `Jwt:Secret`.
3. Restore and build:
   ```bash
   dotnet restore
   dotnet build
   ```
4. Run:
   ```bash
   dotnet run
   ```

`DatabaseInitializer` executes `Data/Database/schema.sql` at startup.

## Authentication Flow

1. Register (`POST /api/auth/register`)
2. Login (`POST /api/auth/login`) -> returns access + refresh token
3. Use access token in `Authorization` header with `Bearer` scheme
4. Refresh (`POST /api/auth/refresh`) rotates refresh token
5. Logout (`POST /api/auth/logout`) revokes current refresh token

## Endpoints

### Auth (Public)
- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `POST /api/auth/logout` (authorized)

### Users (Authorized)
- `GET /api/users/profile`
- `PUT /api/users/profile`
- `POST /api/users/change-password`

### Admin (Admin role)
- `GET /api/admin/users?pageNumber=1&pageSize=10`
- `GET /api/admin/users/{id}`
- `PUT /api/admin/users/{id}/role`
- `PUT /api/admin/users/{id}/status`
- `DELETE /api/admin/users/{id}`
- `GET /api/admin/stats`

## Security Notes

- Passwords are hashed using PBKDF2 with SHA256 (`Utilities/PasswordHasher.cs`).
- Refresh tokens are stored server-side and revoked on rotation/logout.
- Global exception middleware normalizes API errors.
- Admin APIs require `[Authorize(Roles = "Admin")]`.
