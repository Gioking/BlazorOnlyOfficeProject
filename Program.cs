using OnlyOfficeBlazor.Components;
using Serilog;

// Configura Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Avvio applicazione OnlyOfficeBlazor");

    var builder = WebApplication.CreateBuilder(args);

    // CONFIGURA KESTREL per ascoltare su tutte le interfacce
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5000); // Ascolta su 0.0.0.0:5000
    });

    // Aggiungi Serilog
    builder.Host.UseSerilog();

    // Add services to the container
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    builder.Services.AddControllers();
    builder.Services.AddHttpClient();

    var app = builder.Build();

    // Configure the HTTP request pipeline
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

    Log.Information("Applicazione OnlyOfficeBlazor avviata e in ascolto su http://0.0.0.0:5000");

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