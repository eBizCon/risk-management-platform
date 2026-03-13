# AGENTS.md

Before starting any task, read and follow ALL rule files in `.windsurf/rules/`.

## Rule Files

Each rule file contains a YAML frontmatter with a `trigger` field that determines when the rule applies:

| Trigger | Meaning |
|---------|---------|
| `glob` | The rule applies **automatically** when files matching the `globs` pattern are being edited. |
| `model_decision` | The rule applies **only** when the task matches the `description` field in the frontmatter. Read the `description` to decide if the rule is relevant to your current task. |

### Backend Rules (glob: src/backend/**)

- **`.windsurf/rules/backend-ddd.md`** — DDD, Clean Architecture, CQRS Dispatcher, Result Pattern, Domain Events. Apply to all backend code changes.
- **`.windsurf/rules/backend-code-style.md`** — C# naming conventions, SOLID, async/await, strong typing. Apply to all backend code changes.

### Frontend Rules (glob: src/frontend/**)

- **`.windsurf/rules/frontend-code-style.md`** — data-testid attributes, TypeScript types, SOLID, async/await. Apply to all frontend code changes.
- **`.windsurf/rules/svelte-5-runes.md`** — $state, $props, $derived, $effect. Apply to all Svelte component changes.
- **`.windsurf/rules/styling-css.md`** — TailwindCSS 4.0, utility-first, mobile-first. Apply to all frontend styling changes.

### Conditional Rules (apply when task matches description)

- **`.windsurf/rules/backend-database.md`** — EF Core, PostgreSQL, Repository Pattern, Migrations. Apply when changing or adding database access logic in the C# backend.
- **`.windsurf/rules/backend-testing.md`** — xUnit, Moq, FluentAssertions, test structure. Apply when writing or modifying C# backend tests.
- **`.windsurf/rules/frontend-testing.md`** — Playwright E2E, Vitest for components. Apply when writing or modifying frontend tests.
