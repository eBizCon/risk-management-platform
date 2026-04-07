var builder = DistributedApplication.CreateBuilder(args);

var pgUser = builder.AddParameter("pg-user", "risk");
var pgPassword = builder.AddParameter("pg-password", "risk", secret: true);

var postgres = builder.AddPostgres("pg", pgUser, pgPassword)
    .WithDataVolume();

var riskDb = postgres.AddDatabase("risk-management");
var customerDb = postgres.AddDatabase("customer-management");

var kcAdmin = builder.AddParameter("kc-admin", "admin");
var kcPassword = builder.AddParameter("kc-password", "admin", secret: true);

#pragma warning disable ASPIRECERTIFICATES001
var keycloak = builder.AddKeycloak("keycloak", 8081, kcAdmin, kcPassword)
    .WithDataVolume()
    .WithRealmImport("../../../dev/keycloak/import")
    .WithoutHttpsCertificate();
#pragma warning restore ASPIRECERTIFICATES001


var rabbitmqUser = builder.AddParameter("rabbitmq-user", "risk");
var rabbitmqPassword = builder.AddParameter("rabbitmq-password", "risk", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging", rabbitmqUser, rabbitmqPassword)
    .WithDataVolume();

var customerApi = builder.AddProject<Projects.CustomerManagement_Api>("customer-api")
    .WithReference(customerDb, "DefaultConnection")
    .WithReference(keycloak)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(keycloak)
    .WaitFor(rabbitmq);

var riskApi = builder.AddProject<Projects.RiskManagement_Api>("risk-api")
    .WithReference(riskDb, "DefaultConnection")
    .WithReference(keycloak)
    .WithReference(customerApi)
    .WithReference(rabbitmq)
    .WaitFor(postgres)
    .WaitFor(keycloak)
    .WaitFor(rabbitmq);

customerApi.WithEnvironment("APPLICATION_SERVICE_URL", riskApi.GetEndpoint("http"));

builder.Build().Run();
