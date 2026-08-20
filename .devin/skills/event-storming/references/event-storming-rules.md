# Event Storming Rules (Alberto Brandolini)

## Element Types and Colors

| Element | Color | Description |
|---------|-------|-------------|
| **Domain Event** | Orange | Past tense. Something that happened in the domain. |
| **Command** | Blue | Imperative. An intention/action to do something. |
| **Actor** | Small Yellow | A human who triggers a command. Never the system itself. |
| **Aggregate** | Large Yellow | Consistency boundary between Command and Event. Accepts commands, checks invariants, produces events. |
| **Policy** | Lilac/Purple | Reactive automation: "Whenever [Event X] happens, then trigger [Command Y]." Connects events to the next command. |
| **Read Model** | Green | Information an actor reads BEFORE issuing a command. Informs decisions. |
| **External System** | Pink | Third-party system outside the bounded context (e.g. payment provider, Schufa). NOT the own client/browser. |
| **Hotspot** | Red | Open questions, conflicts, unknowns, risks. |
| **Invariant** | Constraint at Aggregate | Business rules the Aggregate checks before producing an event. NOT a Policy. |

## Canonical Linking Grammar

```
Actor → (reads) Read Model → (issues) Command → Aggregate → Domain Event → Policy → Command → Aggregate → Event → ...
```

### Linking Rules

1. **Actor + Read Model → Command**: Actor reads a Read Model, then decides to issue a Command.
2. **Command → Aggregate → Event**: Command goes to an Aggregate. Aggregate checks invariants and produces a Domain Event.
3. **Event → Policy → Command**: Event triggers a Policy, which reactively issues the next Command.
4. **External System → Event**: External system can produce events entering the bounded context.
5. **Command → External System**: A command can invoke an external system.

### What is NOT allowed

- System/Server as an Actor (the system is what is being modeled)
- Static business rules as Policies (those are Invariants on the Aggregate)
- Implementation details as Policies (CSV format, file naming — those are Invariants/Specifications)
- Read Models as outputs in the middle of the flow (they are inputs BEFORE a command)
- Technical events that have no domain meaning (e.g. "ParameterValidated")
- Aggregates missing between Command and Event
- Commands triggered "by the system" (use Policies to connect events to commands)

## Document Template

```markdown
# Event Storming: [Title]

## Context
[Brief description of the use case]

---

## 1. Actor (Small Yellow Sticky Notes)
| Actor | Description |
|-------|-------------|
| **[Name]** | [Role description] |

---

## 2. Read Model (Green Sticky Notes)
Read Models stand BEFORE the Command — they provide the Actor with information to make a decision.

| # | Read Model | Description | Fields |
|---|------------|-------------|--------|
| R1 | **[Name]** | [What the actor sees before deciding] | [field list] |

---

## 3. Commands (Blue Sticky Notes)
| # | Command | Triggered by | Description |
|---|---------|-------------|-------------|
| C1 | **[Name]** | Actor: [Name] | [Description] |
| C2 | **[Name]** | Policy: P1 | [Description] |

---

## 4. Aggregate (Large Yellow Sticky Note)
The Aggregate sits BETWEEN Command and Event. It accepts the command, checks invariants, and produces the Domain Event.

| Aggregate | Description |
|-----------|-------------|
| **[Name]** | [Consistency boundary description] |

### Invariants (Business Rules at the Aggregate)
Invariants are NOT Policies. They are rules the Aggregate checks before producing an event.

| # | Invariant | Description |
|---|-----------|-------------|
| I1 | **[Name]** | [Rule description] |

---

## 5. Domain Events (Orange Sticky Notes)
Domain Events describe domain-relevant state changes in past tense.

| # | Event | Produced by | Description |
|---|-------|-------------|-------------|
| E1 | **[PastTenseName]** | Aggregate: [Name] (after C1) | [Description] |

---

## 6. Policies (Lilac Sticky Notes)
Policies are reactive automations: "Whenever [Event] occurs, trigger [Command]."

| # | Policy | When Event... | Then Command... |
|---|--------|---------------|-----------------|
| P1 | **[Description]** | E1: [Name] | C2: [Name] |

---

## 7. Hotspots / Open Questions (Red Sticky Notes)
| # | Hotspot | Question / Risk |
|---|---------|-----------------|
| H1 | **[Topic]** | [Question] |

---

## 8. Bounded Context
| Context | Responsibility |
|---------|---------------|
| **[Name]** | [Description] |

---

## 9. Event Flow (Canonical Grammar)
[ASCII flow diagram following: Actor → Read Model → Command → Aggregate → Event → Policy → Command → Aggregate → Event]

---

## 10. Linking Summary
| From | Relationship | To |
|------|-------------|-----|
| **[Actor]** (Actor) | reads | **R1** (Read Model) |
| **[Actor]** (Actor) | triggers | **C1** (Command) |
| **C1** (Command) | goes to | **[Aggregate]** (Aggregate) |
| **[Aggregate]** (Aggregate) | checks invariants, produces | **E1** or **E_err** (Event) |
| **E1** (Event) | triggers | **P1** (Policy) |
| **P1** (Policy) | triggers | **C2** (Command) |
```

## FigJam Diagram Color Mapping

When generating a Mermaid flowchart for FigJam (flowchart TB):

| Element | Fill Color | Text Color |
|---------|-----------|------------|
| Actor | `#FFD700` | `#333` |
| Read Model | `#2ECC71` | `#fff` |
| Command | `#4A90D9` | `#fff` |
| Aggregate | `#FFFACD` | `#333` |
| Domain Event | `#FF8C00` | `#fff` |
| Error Event | `#FF4444` | `#fff` |
| Policy | `#9B59B6` | `#fff` |
| Hotspot | `#FF6B6B` | `#fff` |

### Mermaid Node Label Convention

Include element type in parentheses for clarity:
- `["Processor (Actor)"]`
- `["R1: Filtered List (Read Model)"]`
- `["C1: Request Export (Command)"]`
- `["Aggregate: ExportProcess"]`
- `["E1: ExportRequested (Event)"]`
- `["P1: When requested, generate (Policy)"]`
