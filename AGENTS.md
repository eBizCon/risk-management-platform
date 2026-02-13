# AGENTS.md

Before starting any task, read and follow ALL rule files in `.windsurf/rules/`.

## Rule Files

Each rule file contains a YAML frontmatter with a `trigger` field that determines when the rule applies:

| Trigger | Meaning |
|---------|---------|
| `always_on` | The rule applies **always**, regardless of the task. |
| `model_decision` | The rule applies **only** when the task matches the `description` field in the frontmatter. Read the `description` to decide if the rule is relevant to your current task. |

### Always Active Rules

- **`.windsurf/rules/backend-architecture.md`** — Repository Pattern, Service Layer, Zod Validation. Apply to all backend code changes.
- **`.windsurf/rules/code-style-guide.md`** — data-testid attributes, TypeScript types, SOLID principles, code quality. Apply to all code changes.

### Conditional Rules (apply when task matches description)

- **`.windsurf/rules/database-schema.md`** — Drizzle ORM, schema definitions, type safety. Apply when changing or adding database access logic.
- **`.windsurf/rules/styling-css.md`** — TailwindCSS 4.0, utility-first, mobile-first. Apply when changing or adding styling.
- **`.windsurf/rules/svelte-5-runes.md`** — $state, $props, $derived, $effect. Apply when working within a Svelte 5 frontend.
- **`.windsurf/rules/testing-rule.md`** — Playwright E2E, Vitest for components. Apply when writing or modifying tests.
