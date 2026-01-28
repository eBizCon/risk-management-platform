# Database Schema & Drizzle Rule

Ensure type safety and consistency across the database layer.

## Schema Definition
- All table definitions must be located in `src/lib/server/db/schema.ts`.
- Use Drizzle's `sqliteTable` and appropriate column types.

## Type Safety
- Export types using Drizzle's inference helpers:
  - `export type Application = typeof applications.$inferSelect;`
  - `export type NewApplication = typeof applications.$inferInsert;`
- Always use these exported types in services and components to ensure end-to-end type safety.

## Enums
- Use Drizzle's `text(..., { enum: [...] })` for fields with a fixed set of values (e.g., statuses, roles).
- Ensure the TypeScript union types for these enums are also exported for use in the frontend.
