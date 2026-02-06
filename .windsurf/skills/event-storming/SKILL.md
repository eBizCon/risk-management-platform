---
name: event-storming
description: Create an Event Storming document from a Use Case following Alberto Brandolini's canonical rules. Use this skill when the user wants to create an Event Storming, model a domain process with Event Storming, generate an Event Storming board, or analyze a Use Case with Event Storming methodology. Optionally generates a visual FigJam board. Ensures correct element types (Actor, Read Model, Command, Aggregate, Invariant, Domain Event, Policy, Hotspot), canonical linking grammar (Actor → Read Model → Command → Aggregate → Event → Policy → Command), and proper separation of Invariants from Policies.
---

# Event Storming from Use Case

Generate a standards-compliant Event Storming document from a Use Case, with optional FigJam visualization.

## Workflow

1. **Gather context** — Analyze the Use Case and explore relevant codebase (DB schema, routes, components, repositories) to understand the domain.
2. **Generate Event Storming document** — Create the Markdown document following the canonical template. Read `references/event-storming-rules.md` for the complete rules, element definitions, linking grammar, and document template.
3. **Ask about FigJam board** — Ask the user if they want a visual FigJam board.
4. **Generate FigJam board** (optional) — Use the `generate_diagram` Figma MCP tool with Mermaid `flowchart TB` syntax. Read `references/event-storming-rules.md` section "FigJam Diagram Color Mapping" for the color scheme and node label conventions.

## Critical Rules

- **Actor**: Only humans. The system itself is NEVER an actor.
- **Read Model**: Always BEFORE the Command. It is input for the actor's decision, not output.
- **Aggregate**: Always BETWEEN Command and Event. It checks invariants and produces events.
- **Invariants**: Business rules at the Aggregate (role checks, validation, format specs). NOT Policies.
- **Policies**: Reactive only — "Whenever [Event], then [Command]". They connect events to commands.
- **Domain Events**: Past tense, domain-relevant only. No technical events (e.g. "ParameterValidated").
- **Linking grammar**: `Actor → Read Model → Command → Aggregate → Event → Policy → Command → ...`

## Output Location

Save the document to `backlog/event-storming-{use-case-slug}.md`.

## References

- **Canonical rules, template, and FigJam colors**: See [references/event-storming-rules.md](references/event-storming-rules.md) — read this BEFORE generating any Event Storming document.
