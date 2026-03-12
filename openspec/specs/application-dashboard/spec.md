# Application Dashboard

## Purpose

This capability provides a visual dashboard for displaying application statistics and charts to authenticated users with valid roles.

## Requirements

### Requirement: Dashboard matches Figma design

The system SHALL implement the dashboard exactly as specified in the Figma design at https://www.figma.com/design/RjZRM5nOmz78YHfuKs4nZ0/Antrags-Dashboard?node-id=1-54

#### Scenario: Visual layout matches Figma

- **WHEN** the dashboard renders
- **THEN** the layout SHALL match the Figma design with two chart containers side-by-side on desktop

#### Scenario: Colors match Figma specification

- **WHEN** the dashboard renders
- **THEN** status colors SHALL match Figma: Entwurf (#3b82f6 blue), Eingereicht (#6b7280 gray), Genehmigt (#10b981 green), Abgelehnt (#ef4444 red)

### Requirement: Dashboard displays role-based statistics

The system SHALL display application statistics filtered by user role on the homepage dashboard.

#### Scenario: Applicant views own application statistics

- **WHEN** an applicant accesses the homepage
- **THEN** the dashboard SHALL show statistics for only their own applications

#### Scenario: Processor views all application statistics

- **WHEN** a processor accesses the homepage
- **THEN** the dashboard SHALL show statistics for all applications in the system

### Requirement: Dashboard displays stats cards

The system SHALL display stat cards showing counts per status category.

#### Scenario: Stats cards show correct counts

- **WHEN** the dashboard loads
- **THEN** four stat cards SHALL display counts for: Entwurf (draft), Eingereicht (submitted + needs_information + resubmitted), Genehmigt (approved), Abgelehnt (rejected)

#### Scenario: Empty state for new users

- **WHEN** a user has no applications
- **THEN** all stat cards SHALL display "0"

### Requirement: Dashboard displays bar chart

The system SHALL display a bar chart titled "Antrag nach Status" showing application counts per status.

#### Scenario: Bar chart renders with data

- **WHEN** the dashboard loads with applications
- **THEN** a bar chart SHALL display with bars for each status category

#### Scenario: Bar chart handles empty data

- **WHEN** no applications exist
- **THEN** the bar chart SHALL display with zero-height bars

### Requirement: Dashboard displays pie chart

The system SHALL display a pie chart titled "Verteilung" showing percentage distribution.

#### Scenario: Pie chart renders with percentages

- **WHEN** the dashboard loads with applications
- **THEN** a pie chart SHALL display showing percentage distribution with labels (e.g., "Entwurf: 33%")

#### Scenario: Pie chart handles empty data

- **WHEN** no applications exist
- **THEN** the pie chart SHALL display as empty or with a placeholder message

### Requirement: Dashboard follows responsive design

The system SHALL render the dashboard responsively for all screen sizes.

#### Scenario: Mobile layout

- **WHEN** viewport width is below 768px
- **THEN** charts SHALL stack vertically and stat cards SHALL display in a 2x2 grid

#### Scenario: Desktop layout

- **WHEN** viewport width is 768px or above
- **THEN** charts SHALL display side-by-side and stat cards SHALL display in a 1x4 row

### Requirement: Dashboard is role-gated

The system SHALL only display the dashboard section to authenticated users with valid roles.

#### Scenario: Unauthenticated user

- **WHEN** an unauthenticated user visits the homepage
- **THEN** the dashboard section SHALL NOT be displayed

#### Scenario: User without role

- **WHEN** an authenticated user without applicant or processor role visits the homepage
- **THEN** the dashboard section SHALL NOT be displayed
