# Styling & CSS Rule

Ensure consistent styling across the application using TailwindCSS and Svelte's scoped styles.

## TailwindCSS
- Use Tailwind utility classes for most styling needs directly in the HTML/Svelte templates.
- Leverage the custom theme variables defined in `src/app.css` (e.g., `text-primary`, `bg-surface`).

## Global Styles (`src/app.css`)
- Define global styles, CSS variables, and complex reusable utility classes (using `@apply`) in `src/app.css`.
- Use this file for:
  - Theme definitions (`@theme`)
  - Typography defaults
  - Common UI patterns (e.g., `.btn-primary`, `.card`)
  - Layout resets

## Component-Specific Styles
- Use the `<style>` tag within Svelte components for styling that is unique to that component.
- Avoid polluting the global `src/app.css` with component-specific logic.
- Prefer Tailwind classes within the `<style>` tag via `@apply` if custom CSS logic is needed but should remain scoped.

## Design Tokens
- Always use the predefined CSS variables for colors, spacing, and shadows to maintain design consistency.
- Reference: `src/app.css` for the authoritative list of brand and status colors.
