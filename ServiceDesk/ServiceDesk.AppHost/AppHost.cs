var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume()
    .AddDatabase("servicedesk");

var apiService = builder.AddProject<Projects.ServiceDesk_ApiService>("servicedesk-apiservice")
    .WithReference(sql)
    .WaitFor(sql);

builder.AddProject<Projects.ServiceDesk_Web>("servicedesk-web")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
