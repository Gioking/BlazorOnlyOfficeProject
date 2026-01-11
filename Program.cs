using BlazorOnlyOfficeProject.Components;
using Serilog;

// Configura Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/onlyoffice-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Avvio dell'applicazione OnlyOffice Blazor");

    var builder = WebApplication.CreateBuilder(args);

    // Aggiungi Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddControllers();

    builder.Services.AddHttpClient();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    app.MapControllers();

    Log.Information("Applicazione configurata, in ascolto su {BaseAddress}", builder.Configuration["urls"] ?? "http://localhost:5000");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Errore fatale durante l'avvio dell'applicazione");
}
finally
{
    Log.CloseAndFlush();
}
