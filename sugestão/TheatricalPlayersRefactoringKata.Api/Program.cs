using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Serilog;
using TheatricalPlayersRefactoringKata.Api.Persistence;
using TheatricalPlayersRefactoringKata.Api.Processing;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "TheatricalPlayers.Api")
        .WriteTo.Console()
        .WriteTo.File(
            path: context.Configuration["Serilog:FilePath"] ?? "logs/api-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true));

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "Theatrical Players API",
            Version = "v1",
            Description = "Enqueues statement jobs, persists them in SQLite and writes XML asynchronously."
        });
    });

    var connectionString = builder.Configuration.GetConnectionString("TheatricalPlayers")
        ?? "Data Source=theatrical-players.db";

    builder.Services.AddDbContext<TheatricalPlayersDbContext>(options =>
        options.UseSqlite(connectionString));

    builder.Services.AddScoped<IStatementJobStore, EfStatementJobStore>();
    builder.Services.AddSingleton<IStatementJobQueue, StatementJobQueue>();
    builder.Services.AddHostedService<StatementProcessingWorker>();

    var app = builder.Build();

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<TheatricalPlayersDbContext>();
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        db.Database.ExecuteSqlRaw("PRAGMA busy_timeout=5000;");
        Log.Information("Database ready at connection {ConnectionString}", connectionString);
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent.ToString());
        };
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Theatrical Players API v1");
        options.RoutePrefix = "swagger";
    });

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public partial class Program;
