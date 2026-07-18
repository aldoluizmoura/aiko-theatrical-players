using System.Text.Json.Serialization;
using TheatricalPlayersRefactoringKata.Api.Processing;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

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
        Description = "Enqueues statement generation jobs that are processed asynchronously and written as XML files."
    });
});

builder.Services.AddSingleton<IStatementJobQueue, StatementJobQueue>();
builder.Services.AddSingleton<IStatementJobStore, InMemoryStatementJobStore>();
builder.Services.AddHostedService<StatementProcessingWorker>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Theatrical Players API v1");
    options.RoutePrefix = "swagger";
});

app.MapControllers();

app.Run();
