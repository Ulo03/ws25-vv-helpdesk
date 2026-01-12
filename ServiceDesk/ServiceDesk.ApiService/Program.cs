using ServiceDesk.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add SQLServer DbContext
builder.AddSqlServerDbContext<ServiceDeskDbContext>("servicedesk");
var app = builder.Build();

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
