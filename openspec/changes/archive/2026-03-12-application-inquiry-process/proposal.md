## Why

The current application workflow supports drafting, submitting, approving, and rejecting applications, but it does not support requesting additional information from applicants during review. This creates a gap in the review process because processors must either reject incomplete applications or proceed without clarifying missing or conflicting information.

## What Changes

- Introduce a structured inquiry process that allows processors to request additional information for applications under review.
- Add new workflow statuses to represent applications waiting for applicant input and applications resubmitted after an inquiry response.
- Allow applicants to view and answer inquiries on their own applications.
- Persist inquiry history so processors and applicants can see the full clarification trail in the application detail view.
- Enforce role-based access and status transition rules for creating and responding to inquiries.

## Capabilities

### New Capabilities
- `application-inquiries`: Enables processors to create inquiries on reviewable applications and applicants to answer them with a visible inquiry history.

### Modified Capabilities
- `application-workflow`: Extend application status handling and workflow rules to support inquiry-related transitions such as `needs_information` and `resubmitted`.

## Impact

- Affected domain model for applications and a new inquiry persistence model.
- Affected applicant and processor application detail views.
- Affected server-side validation, authorization, and workflow transition logic.
- New or updated API/form actions for creating inquiries and answering them.
- Additional tests for workflow rules, authorization behavior, and UI flow.
