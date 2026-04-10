---
trigger: glob
globs: src/frontend/**
---
# Styling & CSS Rule

Ensure consistent styling across the application using TailwindCSS 4.0 and Svelte's scoped styles.

## TailwindCSS 4.0 (@theme)
- Use the `@theme` block in `src/app.css` as the **Single Source of Truth** for all design tokens (colors, spacing, etc.).
- Avoid defining variables in `:root` and mapping them to `@theme`. Define them directly in `@theme`.
- Tailwind 4.0 automatically makes these variables available as CSS variables (e.g., `--color-brand-primary`).

## Utility-First (Priority 1)
- **Always prefer Tailwind utility classes** over custom CSS in components.
- Avoid using `var(--color-...)` directly in a component's `<style>` tag if a corresponding Tailwind utility exists (e.g., use `text-brand-primary` instead of `color: var(--color-brand-primary)`).
- Use Tailwind modifiers (`hover:`, `focus:`, `disabled:`, etc.) for state-based styling.

## Global Styles Pattern (Priority 2)
- Define reusable UI patterns (e.g., `.btn-primary`, `.card`) in `src/app.css` using `@apply`.
- This keeps components clean while maintaining consistent design patterns.

## Component-Specific Styles (Last Resort)
- Only use the `<style>` tag in Svelte components for complex layout logic or third-party overrides that cannot be handled by Tailwind utilities.
- Do NOT use the `<style>` tag for simple color or spacing changes.

## Mobile First
- Always write mobile-first styles using Tailwind utilities.
- Use `@media` queries only when necessary for larger screens.
- Use Tailwind's responsive modifiers (`md:`, `lg:`, etc.) for responsive design.

## Mobile Responsive Patterns

- **Minimum Supported Width**: Design for minimum width of 360px (small phones).
- **Tabular Views on Mobile**: Tables should default to card layout on mobile with a toggle to switch between card and table views.
- **Card/Table Toggle**: Every tabular view must have a toggle control available for switching between card and table layouts.
- **Card Layout**: Use `.mobile-card` utility class for mobile card views (defined in app.css).
- **Touch Targets**: Ensure minimum touch target size of 44x44px for buttons and interactive elements on mobile.
