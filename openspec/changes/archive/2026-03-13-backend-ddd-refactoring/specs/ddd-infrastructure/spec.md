## ADDED Requirements

### Requirement: ApplicationRepository implements IApplicationRepository
The Infrastructure layer SHALL provide `ApplicationRepository` implementing `IApplicationRepository` from the Domain layer. It SHALL use EF Core with `ApplicationDbContext` for all persistence operations. It SHALL contain NO business logic — only data access and mapping.

#### Scenario: Load application with inquiries
- **WHEN** `GetByIdAsync(id)` is called
- **THEN** the Application aggregate SHALL be returned with its Inquiries collection eagerly loaded

#### Scenario: Load applications by user
- **WHEN** `GetByUserAsync(email, status?)` is called
- **THEN** all applications matching the user email (and optional status filter) SHALL be returned

#### Scenario: Paginated query for processor
- **WHEN** `GetAllPaginatedAsync(status?, page, pageSize)` is called
- **THEN** a tuple of (items, totalCount) SHALL be returned with applications ordered by CreatedAt descending

#### Scenario: Save new application
- **WHEN** `AddAsync(application)` followed by `SaveChangesAsync()` is called
- **THEN** the application and its inquiries SHALL be persisted to the database

#### Scenario: Remove application
- **WHEN** `RemoveAsync(application)` followed by `SaveChangesAsync()` is called
- **THEN** the application SHALL be deleted from the database

#### Scenario: Processor stats query
- **WHEN** processor stats are requested
- **THEN** the repository SHALL return counts for total, submitted, approved, and rejected applications

#### Scenario: Dashboard stats query
- **WHEN** dashboard stats are requested with optional user email filter
- **THEN** the repository SHALL return counts for draft, submitted (including needs_information and resubmitted), approved, and rejected applications

### Requirement: EF Core entity type configurations
Entity configurations SHALL be extracted into dedicated `IEntityTypeConfiguration<T>` classes: `ApplicationConfiguration` and `ApplicationInquiryConfiguration`. They SHALL map to the same database tables and columns as the current `OnModelCreating` configuration. Value object conversions SHALL be configured here.

#### Scenario: ApplicationStatus value conversion
- **WHEN** an Application with Status = Submitted is persisted
- **THEN** the database column `status` SHALL contain the string `"submitted"`

#### Scenario: EmploymentStatus value conversion
- **WHEN** an Application with EmploymentStatus = SelfEmployed is persisted
- **THEN** the database column `employment_status` SHALL contain the string `"self_employed"`

#### Scenario: TrafficLight value conversion
- **WHEN** an Application with TrafficLight = Green is persisted
- **THEN** the database column `traffic_light` SHALL contain the string `"green"`

#### Scenario: ScoringReasons serialization
- **WHEN** an Application with ScoringResult containing Reasons is persisted
- **THEN** the database column `scoring_reasons` SHALL contain JSON-serialized reasons (same format as current)

#### Scenario: Table and column names unchanged
- **WHEN** entities are mapped via the new configurations
- **THEN** table names (`applications`, `application_inquiries`) and all column names SHALL match the existing schema exactly

### Requirement: ApplicationDbContext adjustments
`ApplicationDbContext` SHALL use `ApplyConfigurationsFromAssembly` to load entity configurations. It SHALL remain in the Infrastructure project.

#### Scenario: Configurations auto-discovered
- **WHEN** the DbContext model is built
- **THEN** all `IEntityTypeConfiguration<T>` implementations in the Infrastructure assembly SHALL be applied

### Requirement: DatabaseSeeder in Infrastructure
`DatabaseSeeder` SHALL remain in the Infrastructure project. It SHALL work with the same database schema and seeding logic.

#### Scenario: Seeding unchanged
- **WHEN** the application starts and migrations are applied
- **THEN** the seeder SHALL populate the same seed data as the current implementation

### Requirement: Infrastructure DI registration
The Infrastructure project SHALL expose an `AddInfrastructure(IServiceCollection, IConfiguration)` extension method that registers `ApplicationDbContext`, `ApplicationRepository` (as `IApplicationRepository`), and `DatabaseSeeder`.

#### Scenario: DI registration
- **WHEN** `AddInfrastructure()` is called in Program.cs
- **THEN** all infrastructure services SHALL be registered with their correct lifetimes (DbContext: Scoped, Repository: Scoped, Seeder: Scoped)
