---
trigger: model_decision
description: When changing or adding database access logic in the C# backend
---

# Backend Database & EF Core Rule

## ORM & Provider

- **Entity Framework Core** with **Npgsql** (PostgreSQL provider).
- `ApplicationDbContext` is located in `RiskManagement.Infrastructure.Persistence`.

## Repository Pattern

- Repository interfaces are defined in the **Domain layer** (e.g., `IApplicationRepository` in `Domain/Aggregates/ApplicationAggregate/`).
- Repository implementations are in **Infrastructure** (`Infrastructure/Persistence/`).
- Repositories MUST operate on aggregate roots — never expose `DbSet` or `DbContext` outside Infrastructure.
- Application layer handlers call repository methods, never `DbContext` directly.

## Entity Configuration

- Use `IEntityTypeConfiguration<T>` classes in `Infrastructure/Persistence/Configurations/`.
- Keep entity configurations separate from the DbContext.

## Migrations

- Migrations are managed via EF Core in `Infrastructure/Persistence/Migrations/`.
- Use `dev/add-migration.sh` to create new migrations.

## Seeding

- Database seeding is handled by `DatabaseSeeder` in `Infrastructure/Seeding/`.
- Seeding runs on application startup in development.

## Value Object & Strongly Typed ID Conversions

- Every Value Object persisted in the database MUST have an EF Core `HasConversion()` configured in the `IEntityTypeConfiguration`.
- Strongly Typed IDs (e.g., `ApplicationId`) MUST have value conversions to their underlying type (e.g., `int`).
- Conversion logic lives exclusively in Infrastructure — the Domain layer knows nothing about EF.

## Type Safety

- Use strongly typed entities from the Domain layer.
- Map between domain entities and database representations within the Infrastructure layer only.

## Read Models (CQRS Read-Side)

- Pure read operations (stats, dashboards, paginated lists, exports) SHOULD NOT load full aggregate objects from the write-model repository.
- Use dedicated read-model queries, projections, or lightweight DTOs mapped directly from the database.
- Read-model query services or repositories are separate from write-model (aggregate) repositories.

## Unit of Work

- EF Core's `DbContext` acts as an implicit Unit of Work.
- If explicit transaction control is needed (e.g., aggregate-spanning operations), use `IUnitOfWork` abstraction defined in the Domain layer with `SaveChangesAsync()` and `BeginTransactionAsync()`.
- Handlers coordinate saves through the Unit of Work — repositories SHOULD NOT call `SaveChangesAsync()` themselves.
