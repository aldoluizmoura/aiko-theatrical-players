using Microsoft.EntityFrameworkCore;
using TheatricalPlayersRefactoringKata.Api.Processing;
using TheatricalPlayersRefactoringKata.Statement;

namespace TheatricalPlayersRefactoringKata.Api.Persistence;

public interface IStatementJobStore
{
    Task AddAsync(StatementJob job, CancellationToken cancellationToken = default);
    Task UpdateAsync(StatementJob job, CancellationToken cancellationToken = default);
    Task<StatementJob?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StatementJob>> GetAllAsync(CancellationToken cancellationToken = default);
}

public class EfStatementJobStore : IStatementJobStore
{
    private readonly TheatricalPlayersDbContext _db;
    private readonly ILogger<EfStatementJobStore> _logger;

    public EfStatementJobStore(TheatricalPlayersDbContext db, ILogger<EfStatementJobStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task AddAsync(StatementJob job, CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(job);
        _db.Statements.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Persisted statement job {JobId} for customer {Customer} with status {Status}",
            job.Id, job.Invoice.Customer, job.Status);
    }

    public async Task UpdateAsync(StatementJob job, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Statements
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == job.Id, cancellationToken);

        if (entity is null)
        {
            _logger.LogWarning("Tried to update missing statement job {JobId}", job.Id);
            return;
        }

        entity.Status = job.Status;
        entity.OutputFilePath = job.OutputFilePath;
        entity.XmlContent = job.XmlContent;
        entity.Error = job.Error;
        entity.CompletedAt = job.CompletedAt;
        entity.TotalAmountInCents = job.TotalAmountInCents;
        entity.TotalCredits = job.TotalCredits;

        if (job.ResultLines is { Count: > 0 })
        {
            _db.StatementLines.RemoveRange(entity.Lines);
            entity.Lines = job.ResultLines
                .Select(line => new StatementLineEntity
                {
                    PlayName = line.Name,
                    AmountInCents = line.AmountInCents,
                    Audience = line.Audience,
                    Credits = line.Credits
                })
                .ToList();
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogDebug("Updated statement job {JobId} to status {Status}", job.Id, job.Status);
    }

    public async Task<StatementJob?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _db.Statements
            .AsNoTracking()
            .Include(x => x.Plays)
            .Include(x => x.Performances)
            .Include(x => x.Lines)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return entity is null ? null : ToJob(entity);
    }

    public async Task<IReadOnlyList<StatementJob>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _db.Statements
            .AsNoTracking()
            .Include(x => x.Plays)
            .Include(x => x.Performances)
            .Include(x => x.Lines)
            .ToListAsync(cancellationToken);

        return entities
            .OrderByDescending(x => x.CreatedAt)
            .Select(ToJob)
            .ToList();
    }

    private static StatementEntity ToEntity(StatementJob job)
    {
        return new StatementEntity
        {
            Id = job.Id,
            Customer = job.Invoice.Customer,
            Status = job.Status,
            OutputFilePath = job.OutputFilePath,
            XmlContent = job.XmlContent,
            Error = job.Error,
            TotalAmountInCents = job.TotalAmountInCents,
            TotalCredits = job.TotalCredits,
            CreatedAt = job.CreatedAt,
            CompletedAt = job.CompletedAt,
            Plays = job.Plays
                .Select(pair => new StatementPlayEntity
                {
                    PlayKey = pair.Key,
                    Name = pair.Value.Name,
                    LinesCount = pair.Value.Lines,
                    Type = pair.Value.Type
                })
                .ToList(),
            Performances = job.Invoice.Performances
                .Select(performance => new StatementPerformanceEntity
                {
                    PlayKey = performance.PlayId,
                    Audience = performance.Audience
                })
                .ToList()
        };
    }

    private static StatementJob ToJob(StatementEntity entity)
    {
        var plays = entity.Plays.ToDictionary(
            play => play.PlayKey,
            play => new Play(play.Name, play.LinesCount, play.Type));

        var invoice = new Invoice(
            entity.Customer,
            entity.Performances
                .Select(performance => new Performance(performance.PlayKey, performance.Audience))
                .ToList());

        return new StatementJob
        {
            Id = entity.Id,
            Invoice = invoice,
            Plays = plays,
            Status = entity.Status,
            OutputFilePath = entity.OutputFilePath,
            XmlContent = entity.XmlContent,
            Error = entity.Error,
            TotalAmountInCents = entity.TotalAmountInCents,
            TotalCredits = entity.TotalCredits,
            CreatedAt = entity.CreatedAt,
            CompletedAt = entity.CompletedAt,
            ResultLines = entity.Lines
                .Select(line => new StatementLine(line.PlayName, line.AmountInCents, line.Audience, line.Credits))
                .ToList()
        };
    }
}
