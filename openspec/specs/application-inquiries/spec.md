# Application Inquiries

## Purpose

This capability manages the inquiry process between processors and applicants during application review. Processors can request additional information, and applicants can respond to those inquiries.

## Requirements

### Requirement: Processor can create an inquiry for a reviewable application
The system SHALL allow a user with role `processor` to create an inquiry for an application that is in status `submitted` or `resubmitted`. Creating an inquiry MUST store the inquiry text, the processor identity, the creation timestamp, and mark the inquiry as open.

#### Scenario: Processor creates inquiry for submitted application
- **WHEN** a processor creates an inquiry for an application in status `submitted`
- **THEN** the system stores the inquiry and associates it with the application

#### Scenario: Processor creates follow-up inquiry after resubmission
- **WHEN** a processor creates an inquiry for an application in status `resubmitted`
- **THEN** the system stores the inquiry as the next inquiry entry in the application's inquiry history

#### Scenario: Processor cannot create inquiry for finalized application
- **WHEN** a processor attempts to create an inquiry for an application in status `approved` or `rejected`
- **THEN** the system MUST reject the action

### Requirement: Applicant can answer an open inquiry on their own application
The system SHALL allow only the applicant who created the application to answer an open inquiry on that application. The response text MUST be non-empty and MUST be stored with a response timestamp.

#### Scenario: Applicant answers open inquiry
- **WHEN** the application owner submits a non-empty answer for an open inquiry
- **THEN** the system stores the answer and marks the inquiry as answered

#### Scenario: Non-owner applicant cannot answer inquiry
- **WHEN** an applicant who is not the application owner attempts to answer an inquiry
- **THEN** the system MUST reject the action with forbidden access

#### Scenario: Empty response is rejected
- **WHEN** the application owner submits an empty inquiry response
- **THEN** the system MUST reject the response as invalid

### Requirement: Inquiry history is visible to authorized participants
The system SHALL retain inquiry history for an application so that processors and the owning applicant can review past inquiries and answers in chronological order.

#### Scenario: Applicant views inquiry history on own application
- **WHEN** the owning applicant opens the application detail view
- **THEN** the system shows all inquiries and answers for that application in chronological order

#### Scenario: Processor views inquiry history during review
- **WHEN** a processor opens an application detail view
- **THEN** the system shows all inquiries and answers for that application in chronological order
