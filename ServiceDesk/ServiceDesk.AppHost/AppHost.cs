var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddPostgres("postgres");

var apiService = builder.AddProject<Projects.ServiceDesk_ApiService>("servicedesk-apiservice")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.ServiceDesk_Web>("servicedesk-web")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();