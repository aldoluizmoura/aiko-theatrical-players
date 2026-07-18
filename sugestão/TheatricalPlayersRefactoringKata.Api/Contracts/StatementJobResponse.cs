using TheatricalPlayersRefactoringKata.Api.Processing;

namespace TheatricalPlayersRefactoringKata.Api.Contracts;

public class StatementJobResponse
{
    public Guid Id { get; set; }
    public StatementJobStatus Status { get; set; }
    public string? OutputFilePath { get; set; }
    public string? Error { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public static StatementJobResponse FromJob(StatementJob job) => new()
    {
        Id = job.Id,
        Status = job.Status,
        OutputFilePath = job.OutputFilePath,
        Error = job.Error,
        CreatedAt = job.CreatedAt,
        CompletedAt = job.CompletedAt
    };
}
