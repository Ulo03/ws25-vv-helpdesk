using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add SQLServer DbContext
builder.AddSqlServerDbContext<ServiceDeskDbContext>("servicedesk");
var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServiceDeskDbContext>();
    await db.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.Run();
