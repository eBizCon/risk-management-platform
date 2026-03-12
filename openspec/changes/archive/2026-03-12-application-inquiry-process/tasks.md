## 1. Schema and domain model

- [x] 1.1 Extend the application status model to include `needs_information` and `resubmitted`
- [x] 1.2 Add persistence for application inquiries with fields for application reference, inquiry text, inquiry status, processor identity, timestamps, and applicant response data
- [x] 1.3 Update shared types, labels, and seed/test fixtures affected by the new status values

## 2. Repository and service workflow

- [x] 2.1 Create an inquiry repository for loading inquiry history, creating inquiries, and storing applicant responses
- [x] 2.2 Add workflow service logic that validates allowed inquiry transitions and coordinates application status updates with inquiry persistence
- [x] 2.3 Update application repository behavior so processor decisions support `resubmitted` applications while preserving existing decision rules

## 3. Server actions and authorization

- [x] 3.1 Add Zod validation schemas for processor inquiry creation and applicant inquiry responses
- [x] 3.2 Add server-side actions or endpoints for processors to create inquiries with `401` and `403` behavior aligned to project auth rules
- [x] 3.3 Add server-side actions or endpoints for applicants to answer inquiries on their own applications with ownership checks and invalid-state handling

## 4. Applicant and processor UI

- [x] 4.1 Update the processor application detail view to display inquiry history and provide a form for creating an inquiry when the application is reviewable
- [x] 4.2 Update the applicant application detail view to display inquiry history and provide a response form when the application is waiting for information
- [x] 4.3 Update application lists and detail pages to show the new statuses clearly in both applicant and processor flows

## 5. Verification

- [ ] 5.1 Add repository or service tests for inquiry creation, response handling, and status transitions
- [ ] 5.2 Add authorization tests covering unauthenticated access, wrong-role access, and foreign-application response attempts
- [ ] 5.3 Add UI or end-to-end tests for the processor inquiry flow and applicant response flow
