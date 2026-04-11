using DocAutomation.Application;
using DocAutomation.Infrastructure;
using DocAutomation.Infrastructure.Persistence;
using DocAutomation.Infrastructure.Persistence.Seeders;
using DocAutomation.Web.Components;
using DocAutomation.Web.Services;
using DocAutomation.Web.Services.Ux;
using Microsoft.EntityFrameworkCore;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSqlServerDbContext<DocAutomationDbContext>("DocAutomation");

builder.Services.AddApplication();
builder.Services.AddInfrastructure();

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddRadzenComponents();

builder.Services.AddScoped<DocAutomation.Web.Services.Ux.ThemeService>();
builder.Services.AddScoped<KeyboardShortcutService>();
builder.Services.AddScoped<UnsavedChangesGuard>();
builder.Services.AddScoped<RecentActionsService>();

builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
});

var app = builder.Build();

app.MapDefaultEndpoints();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DocAutomationDbContext>();
    db.Database.Migrate();

    if (app.Environment.IsDevelopment())
    {
        await TemplateSeeder.SeedAsync(db);
    }
}

app.Run();
