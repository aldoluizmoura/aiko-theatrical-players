using TheatricalPlayersRefactoringKata.Api.Persistence;
using TheatricalPlayersRefactoringKata.Formatting;
using TheatricalPlayersRefactoringKata.Statement;
using TheatricalPlayersRefactoringKata.Statement.Interfaces;
using Serilog.Context;

namespace TheatricalPlayersRefactoringKata.Api.Processing;

public class StatementProcessingWorker : BackgroundService
{
    private readonly IStatementJobQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatementProcessingWorker> _logger;
    private readonly StatementGenerator _generator = new();
    private readonly IStatementFormatter _xmlFormatter = new XmlStatementFormatter();

    public StatementProcessingWorker(
        IStatementJobQueue queue,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<StatementProcessingWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var outputDirectory = _configuration.GetValue<string>("StatementOutput:Directory")
            ?? Path.Combine(AppContext.BaseDirectory, "statements-output");

        Directory.CreateDirectory(outputDirectory);
        _logger.LogInformation("Statement worker started. Output directory: {OutputDirectory}", outputDirectory);

        await foreach (var job in _queue.ReadAllAsync(stoppingToken))
        {
            await ProcessJobAsync(job, outputDirectory, stoppingToken);
        }
    }

    private async Task ProcessJobAsync(StatementJob job, string outputDirectory, CancellationToken cancellationToken)
    {
        using var _ = LogContext.PushProperty("JobId", job.Id);
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<IStatementJobStore>();

        job.Status = StatementJobStatus.Processing;
        await store.UpdateAsync(job, cancellationToken);
        _logger.LogInformation("Processing statement job for customer {Customer}", job.Invoice.Customer);

        try
        {
            var statement = _generator.Generate(job.Invoice, job.Plays);
            var xml = _xmlFormatter.Format(statement);
            var filePath = Path.Combine(outputDirectory, $"{job.Id}.xml");

            await File.WriteAllTextAsync(filePath, xml, cancellationToken);

            job.Status = StatementJobStatus.Completed;
            job.OutputFilePath = filePath;
            job.XmlContent = xml;
            job.TotalAmountInCents = statement.TotalAmountInCents;
            job.TotalCredits = statement.TotalCredits;
            job.ResultLines = statement.Lines;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await store.UpdateAsync(job, cancellationToken);

            _logger.LogInformation(
                "Statement job completed. File={FilePath}, TotalAmountInCents={TotalAmountInCents}, TotalCredits={TotalCredits}",
                filePath,
                statement.TotalAmountInCents,
                statement.TotalCredits);
        }
        catch (Exception ex)
        {
            job.Status = StatementJobStatus.Failed;
            job.Error = ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            await store.UpdateAsync(job, cancellationToken);

            _logger.LogError(ex, "Statement job failed for customer {Customer}", job.Invoice.Customer);
        }
    }
}
