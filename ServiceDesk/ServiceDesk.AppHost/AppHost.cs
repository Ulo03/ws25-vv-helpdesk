var builder = DistributedApplication.CreateBuilder(args);

var postgresPassword = builder.AddParameter("postgres-password", secret: true);

var postgres = builder.AddPostgres("postgres", password: postgresPassword)
    .WithEndpoint(port: 52748, targetPort: 5432, name: "postgres");

var database = postgres.AddDatabase("servicedesk");

var apiService = builder.AddProject<Projects.ServiceDesk_ApiService>("servicedesk-apiservice")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.ServiceDesk_Web>("servicedesk-web")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
