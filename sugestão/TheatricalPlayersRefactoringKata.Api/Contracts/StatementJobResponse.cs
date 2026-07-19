using TheatricalPlayersRefactoringKata.Api.Processing;
using TheatricalPlayersRefactoringKata.Statement;

namespace TheatricalPlayersRefactoringKata.Api.Contracts;

public class StatementJobResponse
{
    public Guid Id { get; set; }
    public string Customer { get; set; } = string.Empty;
    public StatementJobStatus Status { get; set; }
    public string? OutputFilePath { get; set; }
    public string? Error { get; set; }
    public int? TotalAmountInCents { get; set; }
    public int? TotalCredits { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public List<PlaySummaryResponse> Plays { get; set; } = [];
    public List<StatementLineResponse> Lines { get; set; } = [];

    public static StatementJobResponse FromJob(StatementJob job) => new()
    {
        Id = job.Id,
        Customer = job.Invoice.Customer,
        Status = job.Status,
        OutputFilePath = job.OutputFilePath,
        Error = job.Error,
        TotalAmountInCents = job.TotalAmountInCents,
        TotalCredits = job.TotalCredits,
        CreatedAt = job.CreatedAt,
        CompletedAt = job.CompletedAt,
        Plays = job.Plays
            .Select(pair => new PlaySummaryResponse
            {
                PlayId = pair.Key,
                Name = pair.Value.Name,
                Lines = pair.Value.Lines,
                Type = pair.Value.Type
            })
            .ToList(),
        Lines = job.ResultLines?
            .Select(line => new StatementLineResponse
            {
                Name = line.Name,
                AmountInCents = line.AmountInCents,
                Audience = line.Audience,
                Credits = line.Credits
            })
            .ToList() ?? []
    };
}

public class PlaySummaryResponse
{
    public string PlayId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Lines { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class StatementLineResponse
{
    public string Name { get; set; } = string.Empty;
    public int AmountInCents { get; set; }
    public int Audience { get; set; }
    public int Credits { get; set; }
}
