var builder = DistributedApplication.CreateBuilder(args);

var isTestMode = IsTestModeEnabled();
var keycloakPort = isTestMode ? GetIntEnvironmentVariable("ASPIRE_TEST_KEYCLOAK_PORT", 8081) : 8081;
var riskApiPort = isTestMode ? GetIntEnvironmentVariable("ASPIRE_TEST_RISK_API_PORT", 5227) : 5227;
var customerApiPort = isTestMode ? GetIntEnvironmentVariable("ASPIRE_TEST_CUSTOMER_API_PORT", 5000) : 5000;

var pgUser = builder.AddParameter("pg-user", "risk");
var pgPassword = builder.AddParameter("pg-password", "risk", secret: true);

var postgres = builder.AddPostgres("pg", pgUser, pgPassword);
if (!isTestMode)
{
    postgres = postgres.WithDataVolume();
}

var riskDb = postgres.AddDatabase("risk-management");
var customerDb = postgres.AddDatabase("customer-management");

var kcAdmin = builder.AddParameter("kc-admin", "admin");
var kcPassword = builder.AddParameter("kc-password", "admin", secret: true);

#pragma warning disable ASPIRECERTIFICATES001
var keycloak = builder.AddKeycloak("keycloak", keycloakPort, kcAdmin, kcPassword);
if (!isTestMode)
{
    keycloak = keycloak.WithDataVolume();
}

keycloak = keycloak
    .WithRealmImport("../../../dev/keycloak/import")
    .WithoutHttpsCertificate();
#pragma warning restore ASPIRECERTIFICATES001


var rabbitmqUser = builder.AddParameter("rabbitmq-user", "risk");
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", "risk", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", rabbitmqUser, rabbitmqPassword);
if (!isTestMode)
{
    rabbitmq = rabbitmq.WithDataVolume();
}

var databaseSeeder = builder.AddProject<Projects.DatabaseSeeder>("database-seeder")
    .WithReference(customerDb, "CustomerConnection")
    .WithReference(riskDb, "RiskConnection")
    .WaitFor(postgres);

var customerApi = builder.AddProject<Projects.CustomerManagement_Api>("customer-api")
    .WithReference(customerDb, "DefaultConnection")
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(databaseSeeder)
    .WaitFor(postgres)
    .WaitFor(keycloak)
    .WaitFor(rabbitmq);

if (isTestMode)
{
    customerApi = customerApi.WithEnvironment("ASPNETCORE_URLS", "http://localhost:" + customerApiPort);
}

var riskApi = builder.AddProject<Projects.RiskManagement_Api>("risk-api")
    .WithReference(riskDb, "DefaultConnection")
    .WithReference(keycloak)
    .WithReference(customerApi)
    .WithReference(rabbitmq)
    .WaitFor(databaseSeeder)
    .WaitFor(postgres)
    .WaitFor(keycloak)
    .WaitFor(rabbitmq);

if (isTestMode)
{
    riskApi = riskApi
        .WithEnvironment("ASPNETCORE_URLS", "http://localhost:" + riskApiPort)
        .WithEnvironment("CUSTOMER_SERVICE_URL", "http://localhost:" + customerApiPort);

    customerApi = customerApi.WithEnvironment("APPLICATION_SERVICE_URL", "http://localhost:" + riskApiPort);
}
else
{
    customerApi.WithEnvironment("APPLICATION_SERVICE_URL", riskApi.GetEndpoint("http"));
}

builder.Build().Run();

static bool IsTestModeEnabled()
{
    var value = Environment.GetEnvironmentVariable("ASPIRE_TEST_MODE");
    if (string.IsNullOrWhiteSpace(value))
    {
        return false;
    }

    return value.Equals("true", StringComparison.OrdinalIgnoreCase)
           || value.Equals("1", StringComparison.OrdinalIgnoreCase);
}

static int GetIntEnvironmentVariable(string name, int fallback)
{
    var value = Environment.GetEnvironmentVariable(name);
    return int.TryParse(value, out var port) ? port : fallback;
}
