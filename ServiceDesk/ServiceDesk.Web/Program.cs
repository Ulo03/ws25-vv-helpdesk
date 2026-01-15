using ServiceDesk.Web.Components;
using ServiceDesk.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// UI Data clients old InMemnory:
// builder.Services.AddScoped<ITicketClient, InMemoryTicketClient>();
// builder.Services.AddScoped<IKnowledgeClient, InMemoryKnowledgeClient>();

// UI Data clients new ApiCalls:
builder.Services.AddHttpClient<ITicketClient, ApiTicketClient>(client =>
{
    client.BaseAddress = new Uri("https+http://servicedesk-apiservice");
});
builder.Services.AddHttpClient<IKnowledgeClient, ApiKnowledgeClient>(client =>
{
    client.BaseAddress = new Uri("https+http://servicedesk-apiservice");
});

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
