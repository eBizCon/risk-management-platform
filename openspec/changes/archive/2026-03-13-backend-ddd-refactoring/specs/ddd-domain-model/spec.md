## ADDED Requirements

### Requirement: Application aggregate root with behavior methods
The `Application` class SHALL be an aggregate root that encapsulates all business logic for credit applications. It SHALL expose behavior methods that enforce status transition invariants. All property setters for business-relevant fields (Status, Score, TrafficLight, ScoringReasons, ProcessorComment, SubmittedAt, ProcessedAt) SHALL be private or internal — state changes SHALL only occur through aggregate methods.

#### Scenario: Create a new application
- **WHEN** an Application is constructed with valid financial data (name, income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault) and a createdBy email
- **THEN** the Application SHALL have Status = Draft, a calculated ScoringResult, and CreatedAt = current UTC time

#### Scenario: Submit a draft application
- **WHEN** `Submit(ScoringService)` is called on an Application with Status = Draft
- **THEN** Status SHALL change to Submitted, SubmittedAt SHALL be set to current UTC, ScoringResult SHALL be recalculated, and an `ApplicationSubmittedEvent` SHALL be added to domain events

#### Scenario: Submit a non-draft application
- **WHEN** `Submit()` is called on an Application with Status != Draft
- **THEN** the aggregate SHALL throw `InvalidStatusTransitionException`

#### Scenario: Approve a submitted or resubmitted application
- **WHEN** `Approve(comment?)` is called on an Application with Status = Submitted or Resubmitted
- **THEN** Status SHALL change to Approved, ProcessorComment SHALL be set, ProcessedAt SHALL be set to current UTC, and an `ApplicationDecidedEvent` SHALL be added

#### Scenario: Reject a submitted or resubmitted application
- **WHEN** `Reject(comment?)` is called on an Application with Status = Submitted or Resubmitted
- **THEN** Status SHALL change to Rejected, ProcessorComment SHALL be set, ProcessedAt SHALL be set to current UTC, and an `ApplicationDecidedEvent` SHALL be added

#### Scenario: Approve or reject an application in wrong status
- **WHEN** `Approve()` or `Reject()` is called on an Application with Status not in (Submitted, Resubmitted)
- **THEN** the aggregate SHALL throw `InvalidStatusTransitionException`

#### Scenario: Update application details
- **WHEN** `UpdateDetails(name, income, fixedCosts, desiredRate, employmentStatus, hasPaymentDefault, ScoringService)` is called on an Application with Status = Draft
- **THEN** the fields SHALL be updated and ScoringResult SHALL be recalculated

#### Scenario: Update application details when not draft
- **WHEN** `UpdateDetails()` is called on an Application with Status != Draft
- **THEN** the aggregate SHALL throw `DomainException`

#### Scenario: Delete a draft application
- **WHEN** `MarkAsDeleted()` is called on an Application with Status = Draft
- **THEN** the operation SHALL succeed (no exception)

#### Scenario: Delete a non-draft application
- **WHEN** `MarkAsDeleted()` is called on an Application with Status != Draft
- **THEN** the aggregate SHALL throw `DomainException`

### Requirement: Application aggregate manages inquiries
The `Application` aggregate SHALL own a collection of `ApplicationInquiry` child entities. All inquiry operations SHALL go through the aggregate root.

#### Scenario: Request information on a submitted/resubmitted application
- **WHEN** `RequestInformation(inquiryText, processorEmail)` is called on an Application with Status = Submitted or Resubmitted, and there is no open inquiry
- **THEN** a new `ApplicationInquiry` SHALL be added with Status = Open, the Application status SHALL change to NeedsInformation, and an `InquiryCreatedEvent` SHALL be added

#### Scenario: Request information when open inquiry exists
- **WHEN** `RequestInformation()` is called and there is already an open inquiry
- **THEN** the aggregate SHALL throw `DomainException` with message indicating an open inquiry already exists

#### Scenario: Request information in wrong application status
- **WHEN** `RequestInformation()` is called on an Application with Status not in (Submitted, Resubmitted)
- **THEN** the aggregate SHALL throw `InvalidStatusTransitionException`

#### Scenario: Answer an open inquiry
- **WHEN** `AnswerInquiry(responseText)` is called on an Application with Status = NeedsInformation and an open inquiry exists
- **THEN** the open inquiry SHALL be updated with ResponseText, RespondedAt, and Status = Answered, and the Application status SHALL change to Resubmitted

#### Scenario: Answer inquiry when no open inquiry exists
- **WHEN** `AnswerInquiry()` is called and there is no open inquiry
- **THEN** the aggregate SHALL throw `DomainException`

### Requirement: ApplicationStatus value object
`ApplicationStatus` SHALL be a value object (or smart enum) with exactly these values: Draft, Submitted, NeedsInformation, Resubmitted, Approved, Rejected. It SHALL map to/from string for database persistence using the existing lowercase snake_case values (draft, submitted, needs_information, resubmitted, approved, rejected).

#### Scenario: Create from valid string
- **WHEN** `ApplicationStatus.From("submitted")` is called
- **THEN** it SHALL return `ApplicationStatus.Submitted`

#### Scenario: Create from invalid string
- **WHEN** `ApplicationStatus.From("invalid_value")` is called
- **THEN** it SHALL throw `ArgumentException`

#### Scenario: Convert to string
- **WHEN** `ApplicationStatus.Submitted.ToString()` or `.Value` is accessed
- **THEN** it SHALL return `"submitted"`

### Requirement: EmploymentStatus value object
`EmploymentStatus` SHALL be a value object with values: Employed, SelfEmployed, Unemployed, Retired. It SHALL map to/from the existing string values (employed, self_employed, unemployed, retired).

#### Scenario: Round-trip conversion
- **WHEN** an EmploymentStatus is created from `"self_employed"` and converted back to string
- **THEN** the result SHALL be `"self_employed"`

### Requirement: TrafficLight value object
`TrafficLight` SHALL be a value object with values: Red, Yellow, Green. It SHALL map to/from the existing string values (red, yellow, green).

#### Scenario: Round-trip conversion
- **WHEN** a TrafficLight is created from `"green"` and converted back to string
- **THEN** the result SHALL be `"green"`

### Requirement: ScoringResult value object
`ScoringResult` SHALL be a value object containing Score (int, 0-100), TrafficLight (TrafficLight value object), and Reasons (list of strings). It SHALL be immutable.

#### Scenario: Create scoring result
- **WHEN** a ScoringResult is created with score=85, trafficLight=Green, reasons=[...]
- **THEN** all properties SHALL be accessible and the object SHALL be immutable

### Requirement: ScoringService as domain service
`ScoringService` SHALL live in the Domain layer with zero external dependencies. It SHALL accept domain types (double for financial values, EmploymentStatus, bool) and return a `ScoringResult` value object. The scoring algorithm SHALL remain identical to the current implementation.

#### Scenario: Calculate score for employed applicant with good financials
- **WHEN** ScoringService calculates for income=5000, fixedCosts=1500, desiredRate=500, employmentStatus=Employed, hasPaymentDefault=false
- **THEN** the result SHALL match the existing scoring algorithm output (score=100, trafficLight=Green)

### Requirement: Domain event infrastructure
`AggregateRoot` base class SHALL maintain a list of `IDomainEvent` instances. Aggregate methods SHALL add events to this list. The events SHALL be clearable after dispatch.

#### Scenario: Aggregate collects events
- **WHEN** `Submit()` is called on an Application
- **THEN** `application.DomainEvents` SHALL contain an `ApplicationSubmittedEvent`

#### Scenario: Events can be cleared
- **WHEN** `ClearDomainEvents()` is called on an aggregate
- **THEN** `DomainEvents` SHALL be empty

### Requirement: Domain exceptions
The Domain layer SHALL define `DomainException` (base) and `InvalidStatusTransitionException` (specific). These SHALL carry meaningful error messages.

#### Scenario: InvalidStatusTransitionException message
- **WHEN** an `InvalidStatusTransitionException` is created with fromStatus=Draft, toStatus=Approved
- **THEN** the message SHALL indicate both the current and attempted status

### Requirement: IApplicationRepository interface
The Domain layer SHALL define `IApplicationRepository` with methods: `GetByIdAsync(int)`, `GetByUserAsync(string email, ApplicationStatus?)`, `GetAllPaginatedAsync(ApplicationStatus?, int page, int pageSize)`, `AddAsync(Application)`, `RemoveAsync(Application)`, `SaveChangesAsync()`. Read-only query methods for stats and export MAY also be included.

#### Scenario: Interface defines persistence contract
- **WHEN** the repository interface is referenced in command handlers
- **THEN** it SHALL be resolvable via DI to the Infrastructure implementation
