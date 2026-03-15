var builder = DistributedApplication.CreateBuilder(args);

var pgUser = builder.AddParameter("pg-user", "risk");
var pgPassword = builder.AddParameter("pg-password", "risk", secret: true);

var postgres = builder.AddPostgres("pg", pgUser, pgPassword)
    .WithDataVolume();

var riskDb = postgres.AddDatabase("risk-management");
var customerDb = postgres.AddDatabase("customer-management");

var kcAdmin = builder.AddParameter("kc-admin", "admin");
var kcPassword = builder.AddParameter("kc-password", "admin", secret: true);

var keycloak = builder.AddKeycloak("keycloak", 8081, kcAdmin, kcPassword)
    .WithDataVolume()
    .WithRealmImport("../../../dev/keycloak/import");

var customerApi = builder.AddProject<Projects.CustomerManagement_Api>("customer-api")
    .WithReference(customerDb, "DefaultConnection")
    .WithReference(keycloak)
    .WaitFor(postgres)
    .WaitFor(keycloak);

var riskApi = builder.AddProject<Projects.RiskManagement_Api>("risk-api")
    .WithReference(riskDb, "DefaultConnection")
    .WithReference(keycloak)
    .WithReference(customerApi)
    .WaitFor(postgres)
    .WaitFor(keycloak);

customerApi.WithEnvironment("APPLICATION_SERVICE_URL", riskApi.GetEndpoint("http"));

builder.Build().Run();
