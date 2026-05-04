# RegisterPanel

Boilerplate de autenticación y registro de usuarios construido con **.NET 9**, arquitectura limpia (Clean Architecture) y PostgreSQL. Listo para usar como base en cualquier proyecto nuevo.

## Características

- Registro de usuario con verificación de email
- Login con JWT
- Recuperación y reset de contraseña
- Reenvío de email de verificación
- Roles (Admin, Client)
- AdminSettings configurable
- Arquitectura limpia (Domain / Application / Infrastructure / Api)
- FluentValidation + MediatR + CQRS
- EF Core + PostgreSQL (snake_case)
- Serilog
- Scalar (documentación OpenAPI)

## Stack

| Capa | Tecnología |
|------|-----------|
| API | ASP.NET Core 9 |
| ORM | Entity Framework Core 9 + Npgsql |
| Auth | ASP.NET Core Identity + JWT Bearer |
| CQRS | MediatR |
| Validación | FluentValidation |
| BD | PostgreSQL 16 |
| Logs | Serilog |
| Docs | Scalar / OpenAPI |

## Estructura

```
src/
├── RegisterPanel.Domain/          # Entidades, interfaces, excepciones
├── RegisterPanel.Application/     # Commands, Queries, DTOs, Behaviors
├── RegisterPanel.Infrastructure/  # EF Core, Repositorios, Servicios, JWT
└── RegisterPanel.Api/             # Controllers, Middleware, Program.cs
```

## Primeros pasos

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (para PostgreSQL)

### 1. Levantar la base de datos

```bash
docker-compose up -d
```

### 2. Configurar `appsettings.Development.json`

El archivo ya viene preconfigurado para desarrollo local. Ajusta si cambias el puerto o la contraseña de PostgreSQL.

### 3. Aplicar migraciones

```bash
dotnet ef database update \
  --project src/RegisterPanel.Infrastructure \
  --startup-project src/RegisterPanel.Api
```

### 4. Arrancar la API

```bash
dotnet run --project src/RegisterPanel.Api
```

La documentación estará disponible en `http://localhost:5000/scalar`.

## Endpoints de autenticación

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/auth/register` | Registro de usuario |
| POST | `/api/v1/auth/login` | Login, devuelve JWT |
| GET | `/api/v1/auth/verify-email` | Verificación de email |
| POST | `/api/v1/auth/resend-verification` | Reenviar email de verificación |
| POST | `/api/v1/auth/forgot-password` | Solicitar reset de contraseña |
| POST | `/api/v1/auth/reset-password` | Resetear contraseña |

## Variables de entorno relevantes

| Clave | Descripción |
|-------|-------------|
| `ConnectionStrings__Default` | Cadena de conexión PostgreSQL |
| `Jwt__Secret` | Clave secreta JWT (mín. 32 chars) |
| `Jwt__Issuer` | Issuer del token |
| `Jwt__Audience` | Audience del token |
| `Admin__Email` | Email del usuario admin inicial |
| `Admin__Password` | Contraseña del usuario admin inicial |
| `App__BaseUrl` | URL base de la API (para links en emails) |

## Licencia

MIT
