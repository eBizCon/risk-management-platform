## ADDED Requirements

### Requirement: Application workflow supports inquiry states
The system SHALL support the additional application statuses `needs_information` and `resubmitted` as part of the application workflow.

#### Scenario: Inquiry requested status is available
- **WHEN** a processor requests additional information for a reviewable application
- **THEN** the application can transition into status `needs_information`

#### Scenario: Resubmitted status is available
- **WHEN** an applicant answers an open inquiry
- **THEN** the application can transition into status `resubmitted`

### Requirement: Application transitions enforce inquiry workflow rules
The system SHALL enforce inquiry-related status transitions so that an application can move from `submitted` to `needs_information`, from `resubmitted` to `needs_information`, and from `needs_information` to `resubmitted` only through the corresponding inquiry actions.

#### Scenario: Submitted application moves to needs information after inquiry
- **WHEN** a processor creates an inquiry for an application in status `submitted`
- **THEN** the system transitions the application to `needs_information`

#### Scenario: Resubmitted application moves to needs information after additional inquiry
- **WHEN** a processor creates a new inquiry for an application in status `resubmitted`
- **THEN** the system transitions the application to `needs_information`

#### Scenario: Needs information application moves to resubmitted after response
- **WHEN** the owning applicant answers the open inquiry for an application in status `needs_information`
- **THEN** the system transitions the application to `resubmitted`

### Requirement: Existing decision flow remains available after resubmission
The system SHALL allow a processor to approve or reject an application after the inquiry response has been submitted and the application is in status `resubmitted`.

#### Scenario: Processor approves resubmitted application
- **WHEN** a processor approves an application in status `resubmitted`
- **THEN** the system transitions the application to `approved`

#### Scenario: Processor rejects resubmitted application
- **WHEN** a processor rejects an application in status `resubmitted`
- **THEN** the system transitions the application to `rejected`
