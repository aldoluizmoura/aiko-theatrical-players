using TheatricalPlayersRefactoringKata.Formatting;
using TheatricalPlayersRefactoringKata.Statement;
using TheatricalPlayersRefactoringKata.Statement.Interfaces;

namespace TheatricalPlayersRefactoringKata.Api.Processing;

public class StatementProcessingWorker : BackgroundService
{
    private readonly IStatementJobQueue _queue;
    private readonly IStatementJobStore _store;
    private readonly IConfiguration _configuration;
    private readonly ILogger<StatementProcessingWorker> _logger;
    private readonly StatementGenerator _generator = new();
    private readonly IStatementFormatter _xmlFormatter = new XmlStatementFormatter();

    public StatementProcessingWorker(
        IStatementJobQueue queue,
        IStatementJobStore store,
        IConfiguration configuration,
        ILogger<StatementProcessingWorker> logger)
    {
        _queue = queue;
        _store = store;
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
        job.Status = StatementJobStatus.Processing;
        _store.Update(job);

        try
        {
            var statement = _generator.Generate(job.Invoice, job.Plays);
            var xml = _xmlFormatter.Format(statement);
            var filePath = Path.Combine(outputDirectory, $"{job.Id}.xml");

            await File.WriteAllTextAsync(filePath, xml, cancellationToken);

            job.Status = StatementJobStatus.Completed;
            job.OutputFilePath = filePath;
            job.CompletedAt = DateTimeOffset.UtcNow;
            _store.Update(job);

            _logger.LogInformation("Statement job {JobId} completed. File: {FilePath}", job.Id, filePath);
        }
        catch (Exception ex)
        {
            job.Status = StatementJobStatus.Failed;
            job.Error = ex.Message;
            job.CompletedAt = DateTimeOffset.UtcNow;
            _store.Update(job);

            _logger.LogError(ex, "Statement job {JobId} failed", job.Id);
        }
    }
}
