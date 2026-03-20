# Context Map - Risk Management Platform

## Overview

This document describes the bounded contexts and their relationships following Domain-Driven Design strategic patterns.

---

## Bounded Contexts

### 1. CustomerManagement Context

**Namespace:** `CustomerManagement.*`  
**Deployment:** `CustomerManagement.Api` (separate service)

#### Aggregates
- **Customer** — Customer entity with personal data, employment status, credit report

#### Value Objects
- `CustomerId` (Strongly Typed ID)
- `Address`
- `PhoneNumber`
- `CreditReport`
- `CustomerStatus` (Active, Archived)

#### Domain Services
- `ICreditReportProvider` — External credit check integration (SCHUFA mock)

#### Domain Events
- `CustomerCreatedEvent`
- `CustomerUpdatedEvent`
- `CustomerArchivedEvent`
- `CustomerActivatedEvent`
- `CreditReportReceivedEvent`

---

### 2. RiskManagement Context

**Namespace:** `RiskManagement.*`  
**Deployment:** `RiskManagement.Api` (separate service)

#### Aggregates
- **Application** — Credit application with scoring, status workflow
- **ScoringConfig** — Configuration for scoring rules with versioning

#### Value Objects
- `ApplicationId` (Strongly Typed ID)
- `ScoringConfigVersionId` (Strongly Typed ID)
- `Money`
- `TrafficLight` (Green, Yellow, Red)
- `ApplicationStatus` (Draft, Submitted, Resubmitted, Approved, Rejected)

#### Domain Services
- `IScoringService` — Calculates score based on income, costs, employment, credit history

#### Domain Events
- `ApplicationSubmittedEvent`
- `ApplicationDecidedEvent`

---

### 3. SharedKernel

**Namespace:** `SharedKernel`  
**Deployment:** Shared project reference (not a service)

#### Shared Building Blocks
- `AggregateRoot<TId>` — Base for all aggregates
- `Entity<TId>` — Base for entities
- `ValueObject` — Base for value objects
- `Enumeration` — Base for enums-as-classes
- `IDomainEvent` — Marker for domain events

#### Shared Value Objects
- `EmailAddress`
- `EmploymentStatus` (Employed, SelfEmployed, Unemployed, Retired, Student)

#### CQRS Infrastructure
- `ICommand<TResult>`, `IQuery<TResult>`
- `IDispatcher` — Custom CQRS dispatcher
- `ICommandHandler<,>`, `IQueryHandler<,>`, `IDomainEventHandler<>`
- `Result<T>` — Result pattern (Success, Failure, NotFound, Forbidden)

---

## Context Relationships

### CustomerManagement → RiskManagement (Customer Data)

| Aspect | Details |
|--------|---------|
| **Direction** | CustomerManagement (Upstream) → RiskManagement (Downstream) |
| **Pattern** | Open Host Service (OHS) + Published Language (PL) |
| **Protocol** | Synchronous HTTP, API Key auth |
| **Endpoint** | `GET /api/internal/customers/{id}` |
| **ACL** | `CustomerServiceClient` in RiskManagement.Infrastructure |
| **Data** | CustomerProfile (Id, Name, EmploymentStatus, CreditReport) |
| **Purpose** | RiskManagement needs customer data for credit applications |

### RiskManagement → CustomerManagement (Application Check)

| Aspect | Details |
|--------|---------|
| **Direction** | RiskManagement (Upstream) → CustomerManagement (Downstream) |
| **Pattern** | Open Host Service (OHS) + Published Language (PL) |
| **Protocol** | Synchronous HTTP, API Key auth |
| **Endpoint** | Internal API for application existence check |
| **ACL** | `IApplicationServiceClient` in CustomerManagement.Application |
| **Purpose** | Prevent customer deletion when applications exist |

---

## Communication Patterns

### Synchronous HTTP

- `/api/internal/customers/{id}` — Customer data lookup
- `/api/internal/applications` — Application existence check

### Shared Database (Schema Separation)

| Context | PostgreSQL Schema |
|---------|-------------------|
| CustomerManagement | `customer` |
| RiskManagement | `public` |

---

## Key Design Decisions

1. **Separate Deployments** — Each bounded context has its own solution and deployment
2. **SharedKernel as Project Reference** — Shared code via project reference, not NuGet
3. **Schema Separation** — Same database, different schemas for isolation
4. **Synchronous HTTP** — Service-to-service via HTTP with API key authentication
5. **ACL on Both Sides** — Each context has an anti-corruption layer for the other
6. **Custom CQRS** — Lightweight dispatcher without MediatR

---

## Mappings Summary

| Context | Upstream | Downstream | Pattern |
|---------|----------|------------|---------|
| CustomerManagement | — | RiskManagement | OHS/PL |
| RiskManagement | — | CustomerManagement | OHS/PL |
| Both | SharedKernel | — | Shared Library |

---

## External Integrations

| Service | Interface | Implementation | Status |
|---------|-----------|----------------|--------|
| SCHUFA Credit Check | `ICreditReportProvider` | `MockSchufaProvider` | Mocked |

---

*Generated: March 2026*
