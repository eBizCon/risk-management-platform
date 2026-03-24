---
trigger: glob
globs: src/backend/**
---

# Backend Code Style Rule

Ensure consistent, clean, and idiomatic C# code across the backend.

## Naming Conventions

- **PascalCase**: public members, methods, properties, classes, interfaces, namespaces
- **_camelCase**: private fields (with underscore prefix)
- **camelCase**: local variables, method parameters
- **I-Prefix**: interfaces (e.g., `IApplicationRepository`, `IDispatcher`)
- File names MUST match the primary type name (e.g., `ApplicationRepository.cs`)

## Type Safety

- Use strong typing everywhere — avoid `object` or `dynamic`.
- Use `record` types for Commands, Queries, DTOs, and Result types.
- Use enums for fixed sets of values.
- Nullable reference types MUST be enabled (`<Nullable>enable</Nullable>`).
- Avoid Primitive Obsession: wrap domain concepts in Value Objects or `record` types instead of using bare primitives. Examples: `Money` instead of `double` for monetary amounts, `EmailAddress` instead of `string` for emails, `ApplicationId` instead of `int` for entity IDs.
- Strongly Typed IDs (e.g., `record ApplicationId(int Value)`) SHOULD be used to prevent mixing IDs of different aggregates at compile time.

## Code Quality

- Follow SOLID principles strictly.
- Keep methods small and focused on a single responsibility.
- Avoid code duplication — extract shared logic into helper classes or base classes.
- Use meaningful, descriptive names for all symbols.
- All async methods MUST use `async/await` — never `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
- All async method names MUST end with `Async` suffix.
- Use `CancellationToken` in all async method signatures.

## File-Scoped Namespaces

- Use file-scoped namespace declarations: `namespace Foo.Bar;` (not block-scoped `namespace Foo.Bar { }`).

## Dependency Injection

- Constructor injection only — no service locator pattern.
- Register services with appropriate lifetimes: Scoped for handlers/repositories, Singleton for stateless services.
