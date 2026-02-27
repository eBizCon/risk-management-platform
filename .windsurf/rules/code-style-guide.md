---
trigger: model_decision
---

# Code Style Guide

## UI implementation requirements

- When implementing or modifying UI, ensure stable `data-testid` attributes exist for:
  - primary actions (buttons/links)
  - form fields and validation messages
  - modals/dialogs
  - tables/lists and their key rows/items
  - navigation elements
- If you add a new interactive element, you MUST add a `data-testid` suitable for E2E testing.

## Naming convention

- Use kebab-case and a consistent prefix: `<area>-<component>-<element>`
  - Examples: `checkout-submit`, `login-email-input`, `users-table-row`, `users-delete-button`
- Avoid dynamic/unstable values in testids (no GUIDs/timestamps). Prefer semantic identifiers.

## Typescript Types

- Use TypeScript for all new code
- Prefer interfaces over types for object shapes
- Use enums for fixed sets of related values
- Use generics for reusable, type-safe components
- Avoid using `any` - use `unknown` instead when type is not known

## Code Quality

- Write clean, readable, and maintainable code
- Follow the SOLID principles
- Keep functions small and focused on a single responsibility
- Avoid code duplication
- Use meaningful variable and function names
- Add comments for complex logic
- Write unit tests for new features
- Run linters and formatters before committing
- All async functions must use async/await pattern not then chains
