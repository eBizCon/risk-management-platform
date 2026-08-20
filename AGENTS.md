# AGENTS.md

This file defines global repository-wide instructions.

## Global Instructions

- Use the nearest `AGENTS.md` in the directory tree as the primary source of local instructions.
- When multiple `AGENTS.md` files apply, the deeper file has precedence for its subtree.
- Keep code, tests, and comments in English.
- Prefer minimal, focused changes that respect the existing architecture.

## Project Intent

- This project demonstrates Domain-Driven Design concepts in a realistic, DDD-proper way.
- Prefer realistic domain modeling over shortcuts, even when using mocks (e.g., `MockSchufaProvider`).
- Preserve bounded-context decoupling between backend contexts.

## Operational Context

- The Azure tenant "Doppelmayr Seilbahnen GmbH" (`1797eb83-13d8-42de-b502-001bdf9452a4`) is **not** relevant for this project.
- Use only the personal subscriptions under tenant `dd01ae66-67c3-46f0-b2b6-f441fd73558b`:
  - "PATRICK HENKELMANN Subscription"
  - "Verbrauchstarif"

## Infrastructure Notes

- The infrastructure supports two alert-to-Devin modes:
  1. Direct webhook via `devinSessionWebhookUrl`.
  2. Bridge mode via an Azure Function when `devinApiUrl` and `alertWebhookToken` are set.
- The Devin alert bridge is deployed as `${prefix}-devin-bridge` and forwards `POST /api/alerts/devin?token=...` to the Devin API.

## Scope Notes

- Backend-specific details are defined in `src/backend/AGENTS.md`.
- Frontend-specific details are defined in `src/frontend/AGENTS.md`.

