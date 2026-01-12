using ServiceDesk.Web.Components;
using ServiceDesk.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// UI Data clients (TODO: Patrick: Change from InMemnory to API calls)
builder.Services.AddScoped<ITicketClient, InMemoryTicketClient>();
builder.Services.AddScoped<IKnowledgeClient, InMemoryKnowledgeClient>();

// Health Check via Aspire Service Discovery
builder.Services.AddHttpClient<IHealthClient, ApiHealthClient>(client =>
{
    client.BaseAddress = new Uri("https+http://servicedesk-apiservice");
});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();