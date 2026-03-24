## MODIFIED Requirements

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
