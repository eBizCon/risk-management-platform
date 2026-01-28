---
trigger: model_decision
description: When you work within an Svelte 5 frontend
---
# Svelte 5 Runes Rule

Always use Svelte 5 Runes for state management and component logic.

## State Management
- Use `$state()` for reactive variables instead of simple let declarations or writable stores for local state.
- Use `$derived()` for computed values that depend on other reactive state.
- Use `$effect()` for side effects, but keep them to a minimum.

## Component Props
- Use `$props()` to define component inputs.
- Never use the old `export let` syntax.
- Example:
  ```svelte
  <script lang="ts">
    let { value, onchange }: { value: string, onchange: (v: string) => void } = $props();
  </script>
  ```

## Event Handling
- Prefer passing callback functions as props (e.g., `onclick`, `onchange`) over the old `on:click` directive.
- This aligns with Svelte 5's shift towards standard attribute-based event handling.

## Snippets
- Use `{#snippet ...}` for reusable UI fragments within a component instead of multiple small components where appropriate.
