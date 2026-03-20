## ADDED Requirements

### Requirement: Scoring config entity with versioned parameters
The system SHALL store scoring configuration as versioned, immutable rows in a `scoring_config_versions` table. Each version SHALL contain all scoring parameters: TrafficLight thresholds (`greenThreshold`, `yellowThreshold`), income ratio tiers (`incomeRatioGood`, `incomeRatioModerate`, `incomeRatioLimited`) with penalties (`penaltyModerateRatio`, `penaltyLimitedRatio`, `penaltyCriticalRatio`), rate affordability tiers (`rateGood`, `rateModerate`, `rateHeavy`) with penalties (`penaltyModerateRate`, `penaltyHeavyRate`, `penaltyExcessiveRate`), and individual penalties (`penaltySelfEmployed`, `penaltyRetired`, `penaltyUnemployed`, `penaltyPaymentDefault`). Each version SHALL record `createdBy`, `createdAt`, and an auto-incrementing `version` number.

#### Scenario: Default config v1 exists after database seed
- **WHEN** the application starts with an empty database
- **THEN** a default scoring config version 1 SHALL be seeded with values matching the current hardcoded thresholds (greenThreshold=75, yellowThreshold=50, incomeRatioGood=0.5, incomeRatioModerate=0.3, incomeRatioLimited=0.1, penaltyModerateRatio=15, penaltyLimitedRatio=30, penaltyCriticalRatio=50, rateGood=0.3, rateModerate=0.5, rateHeavy=0.7, penaltyModerateRate=10, penaltyHeavyRate=25, penaltyExcessiveRate=40, penaltySelfEmployed=10, penaltyRetired=5, penaltyUnemployed=35, penaltyPaymentDefault=25)

#### Scenario: New version created on config update
- **WHEN** a Risk Manager saves updated scoring parameters
- **THEN** a new immutable row SHALL be inserted with an incremented version number
- **THEN** the previous version SHALL remain unchanged in the database

### Requirement: Config validation constraints
The system SHALL validate scoring config parameters before saving. Validation rules: `greenThreshold` MUST be greater than `yellowThreshold`. Both thresholds MUST be between 1 and 99 inclusive. All ratio values MUST be between 0.01 and 0.99 inclusive. `incomeRatioGood` MUST be greater than `incomeRatioModerate`, which MUST be greater than `incomeRatioLimited`. `rateGood` MUST be less than `rateModerate`, which MUST be less than `rateHeavy`. All penalty values MUST be non-negative integers between 0 and 100.

#### Scenario: Valid config is accepted
- **WHEN** a Risk Manager submits config with greenThreshold=80, yellowThreshold=60, and all other values within valid ranges respecting ordering constraints
- **THEN** the system SHALL save a new config version

#### Scenario: Invalid threshold ordering is rejected
- **WHEN** a Risk Manager submits config with greenThreshold=40 and yellowThreshold=60
- **THEN** the system SHALL return a validation error indicating greenThreshold must be greater than yellowThreshold

#### Scenario: Invalid ratio ordering is rejected
- **WHEN** a Risk Manager submits config with incomeRatioGood=0.2 and incomeRatioModerate=0.4
- **THEN** the system SHALL return a validation error indicating incomeRatioGood must be greater than incomeRatioModerate

#### Scenario: Negative penalty is rejected
- **WHEN** a Risk Manager submits config with penaltyUnemployed=-5
- **THEN** the system SHALL return a validation error indicating penalty values must be non-negative

### Requirement: ScoringService reads config parameters
The `IScoringService.CalculateScore()` method SHALL accept a scoring config as an additional parameter. The scoring algorithm SHALL use config values instead of hardcoded constants. The scoring logic (algorithm structure) SHALL remain unchanged — only the parameter source changes. The `HasPaymentDefault` and `CreditScore` inputs for scoring SHALL be read from the `Application.CreditReport` value object instead of being passed as separate constructor parameters sourced from the customer profile.

#### Scenario: Score calculated with custom config
- **WHEN** an application is scored with a config where greenThreshold=80 and yellowThreshold=60
- **THEN** a score of 75 SHALL result in TrafficLight Yellow (not Green as with default config)

#### Scenario: Score calculated with default config matches current behavior
- **WHEN** an application is scored with the default config v1
- **THEN** the result SHALL be identical to the current hardcoded scoring behavior

#### Scenario: Scoring uses credit data from application's own CreditReport
- **WHEN** an application is created and scored
- **THEN** the scoring SHALL use `HasPaymentDefault` and `CreditScore` from the application's `CreditReport` value object
- **THEN** the scoring SHALL NOT depend on customer profile credit report data

### Requirement: Application records scoring config version
The `Application` entity SHALL store a nullable `ScoringConfigVersionId` referencing the config version used for scoring. Every new scoring (create, update, submit, resubmit, rescore) SHALL set this field to the current config version ID.

#### Scenario: New application records config version
- **WHEN** an applicant creates or submits an application
- **THEN** the application's `ScoringConfigVersionId` SHALL be set to the current active config version

#### Scenario: Legacy applications have null config version
- **WHEN** querying an application that was scored before config versioning was introduced
- **THEN** `ScoringConfigVersionId` SHALL be null

### Requirement: Get current scoring config via API
The system SHALL provide a `GET /api/scoring-config` endpoint that returns the current (latest) scoring config version with all parameters. This endpoint SHALL require the `risk_manager` role.

#### Scenario: Risk Manager retrieves current config
- **WHEN** an authenticated Risk Manager calls `GET /api/scoring-config`
- **THEN** the system SHALL return the latest config version with all parameter values, version number, createdBy, and createdAt

#### Scenario: Non-Risk-Manager is forbidden
- **WHEN** a user with role `processor` calls `GET /api/scoring-config`
- **THEN** the system SHALL return HTTP 403

### Requirement: Update scoring config via API
The system SHALL provide a `PUT /api/scoring-config` endpoint that creates a new config version. This endpoint SHALL require the `risk_manager` role. The request body SHALL contain all scoring parameters. The system SHALL validate all parameters before saving.

#### Scenario: Successful config update
- **WHEN** a Risk Manager sends valid updated parameters to `PUT /api/scoring-config`
- **THEN** a new config version SHALL be created and returned with the new version number

#### Scenario: Validation failure returns error details
- **WHEN** a Risk Manager sends invalid parameters to `PUT /api/scoring-config`
- **THEN** the system SHALL return HTTP 400 with specific validation error messages

### Requirement: Rescore open applications via API
The system SHALL provide a `POST /api/scoring-config/rescore` endpoint that recalculates scores for all open applications using the current config version. Open applications are those with status `submitted`, `resubmitted`, or `needs_information`. Applications with status `draft`, `approved`, or `rejected` SHALL NOT be rescored. This endpoint SHALL require the `risk_manager` role.

#### Scenario: Successful rescoring of open applications
- **WHEN** a Risk Manager triggers rescore and there are 5 submitted and 3 approved applications
- **THEN** only the 5 submitted applications SHALL be rescored with the current config
- **THEN** the 3 approved applications SHALL remain unchanged
- **THEN** the response SHALL include the count of rescored applications

#### Scenario: Rescoring updates config version on applications
- **WHEN** open applications are rescored with config version 3
- **THEN** each rescored application's `ScoringConfigVersionId` SHALL be updated to version 3

#### Scenario: No open applications to rescore
- **WHEN** a Risk Manager triggers rescore but no open applications exist
- **THEN** the system SHALL return success with a count of 0

### Requirement: Risk Manager scoring config UI
The system SHALL provide a page at `/risk-manager/scoring-config` accessible only to users with the `risk_manager` role. The page SHALL display the current config parameters in an editable form, the current version number and metadata, a "Save" action to create a new version, and a "Rescore open applications" action. The form SHALL show validation errors inline.

#### Scenario: Risk Manager views config page
- **WHEN** a Risk Manager navigates to `/risk-manager/scoring-config`
- **THEN** the page SHALL display all current scoring parameters in editable form fields

#### Scenario: Risk Manager saves updated config
- **WHEN** a Risk Manager edits parameters and clicks Save
- **THEN** a new config version SHALL be created
- **THEN** the page SHALL show the updated version number

#### Scenario: Risk Manager triggers rescoring
- **WHEN** a Risk Manager clicks "Offene Anträge neu bewerten"
- **THEN** the system SHALL rescore all open applications
- **THEN** the page SHALL display a confirmation with the count of rescored applications

#### Scenario: Unauthorized user sees access denied
- **WHEN** a user without `risk_manager` role navigates to `/risk-manager/scoring-config`
- **THEN** the page SHALL display "Keine Berechtigung" with a link to login
