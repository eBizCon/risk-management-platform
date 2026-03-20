## ADDED Requirements

### Requirement: EmploymentStatus on Customer aggregate
The Customer aggregate SHALL have a required `EmploymentStatus` property of type `EmploymentStatus` (Enumeration VO from SharedKernel). Valid values: `employed`, `self_employed`, `unemployed`, `retired`. The field SHALL be required on customer creation and update.

#### Scenario: Customer created with employment status
- **WHEN** a customer is created with `EmploymentStatus = "employed"`
- **THEN** the Customer's `EmploymentStatus` property SHALL be set to `EmploymentStatus.Employed`

#### Scenario: Customer creation without employment status
- **WHEN** a customer creation request omits the `employmentStatus` field
- **THEN** the system SHALL return a validation error "Bitte wählen Sie einen Beschäftigungsstatus"

#### Scenario: Invalid employment status value
- **WHEN** a customer creation request includes `employmentStatus = "student"`
- **THEN** the system SHALL return a validation error "Bitte wählen Sie einen gültigen Beschäftigungsstatus"

### Requirement: EmploymentStatus in Customer create/update flows
The `CustomerCreateDto` and `CustomerUpdateDto` SHALL include an `employmentStatus` (string) field. The Customer API endpoints `POST /api/customers` and `PUT /api/customers/{id}` SHALL accept and persist the employment status.

#### Scenario: Create customer with employment status via API
- **WHEN** an applicant calls `POST /api/customers` with `employmentStatus = "self_employed"`
- **THEN** the customer SHALL be created with `EmploymentStatus = SelfEmployed`
- **THEN** the response SHALL include `employmentStatus: "self_employed"`

#### Scenario: Update customer employment status via API
- **WHEN** an applicant calls `PUT /api/customers/{id}` with `employmentStatus = "retired"`
- **THEN** the customer's EmploymentStatus SHALL be updated to `Retired`

### Requirement: EmploymentStatus in Customer API responses
The `CustomerResponse` SHALL include `employmentStatus` (string) field. The internal customer response SHALL also include this field.

#### Scenario: CustomerResponse includes employment status
- **WHEN** a customer is queried via `GET /api/customers/{id}`
- **THEN** the response SHALL include `employmentStatus` with the current value (e.g., "employed")

#### Scenario: Internal customer response includes employment status
- **WHEN** the Application Service calls `GET /api/internal/customers/{id}`
- **THEN** the response SHALL include `employmentStatus`

### Requirement: EmploymentStatus Enumeration in SharedKernel
The `EmploymentStatus` Enumeration VO SHALL be moved from `RiskManagement.Domain.ValueObjects` to `SharedKernel` so both bounded contexts can reference it. The class, values, and behavior SHALL remain unchanged.

#### Scenario: EmploymentStatus accessible from both contexts
- **WHEN** `CustomerManagement.Domain` or `RiskManagement.Domain` references `EmploymentStatus`
- **THEN** both SHALL resolve to the same SharedKernel type

### Requirement: EmploymentStatus in Customer UI forms
The Customer create form (`/customers/new`) and edit form (`/customers/{id}/edit`) SHALL include a dropdown for selecting the employment status with options: Angestellt, Selbstständig, Arbeitslos, Rentner.

#### Scenario: Employment status dropdown on create form
- **WHEN** an applicant opens the customer create form
- **THEN** the form SHALL display a required "Beschäftigungsstatus" dropdown with all four options

#### Scenario: Employment status pre-selected on edit form
- **WHEN** an applicant opens the customer edit form for a customer with `EmploymentStatus = "retired"`
- **THEN** the dropdown SHALL pre-select "Rentner"

### Requirement: EF Core mapping for EmploymentStatus on Customer
The `employment_status` column SHALL be added to the `customer.customers` table as a required varchar column storing the Enumeration string value (e.g., "employed", "self_employed").

#### Scenario: Employment status persisted
- **WHEN** a customer with `EmploymentStatus = SelfEmployed` is saved
- **THEN** the `employment_status` column SHALL contain "self_employed"
