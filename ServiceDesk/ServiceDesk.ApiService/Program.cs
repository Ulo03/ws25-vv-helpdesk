using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using ServiceDesk.ApiService.Endpoints;
using ServiceDesk.Data;
using ServiceDesk.Data.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add SQLServer DbContext
builder.AddSqlServerDbContext<ServiceDeskDbContext>("servicedesk");

// Serialise enums as strings
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ServiceDeskDbContext>();
    await db.Database.MigrateAsync();
    await SeedDefaultAdminAsync(db);
}

app.MapDefaultEndpoints();

app.MapGet("/", () => "Hello World!");

app.MapTicketEndpoints();
app.MapKnowledgeEndpoints();

app.Run();

static async Task SeedDefaultAdminAsync(ServiceDeskDbContext db)
{
    var anyUserExists = await db.Users.AsNoTracking().AnyAsync();
    if (anyUserExists)
    {
        return;
    }

    var now = DateTime.UtcNow;

    db.Users.Add(new User
    {
        Id = Guid.NewGuid(),
        Username = "admin",
        Role = "Admin",
        PasswordHash = "DEV-ONLY", // Replace later with real authentication/hashing
        CreatedAt = now
    });

    await db.SaveChangesAsync();
}
