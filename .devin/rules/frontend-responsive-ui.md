---
description: Responsive UI guidelines for the frontend
trigger: glob
globs:
  - src/frontend/**/*.svelte
  - src/frontend/**/*.css
  - src/frontend/**/*.ts
# trigger: always_on | manual | model_decision | glob
---

# Responsive UI

Follow these responsive-design decisions for all frontend work.

- Minimum supported viewport width is **360px**.
- All pages and components are in scope for responsive behavior.
- Tabular views must default to a **card layout on mobile**.
- Every table must provide a **toggle to switch between card and table view**.
- Build mobile-first with Tailwind utility classes, then enhance for larger viewports.
