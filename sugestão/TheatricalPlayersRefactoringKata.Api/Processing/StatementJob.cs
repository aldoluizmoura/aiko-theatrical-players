using TheatricalPlayersRefactoringKata.Statement;

namespace TheatricalPlayersRefactoringKata.Api.Processing;

public class StatementJob
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required Invoice Invoice { get; init; }
    public required Dictionary<string, Play> Plays { get; init; }
    public StatementJobStatus Status { get; set; } = StatementJobStatus.Queued;
    public string? OutputFilePath { get; set; }
    public string? XmlContent { get; set; }
    public string? Error { get; set; }
    public int? TotalAmountInCents { get; set; }
    public int? TotalCredits { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? CompletedAt { get; set; }
    public IReadOnlyList<StatementLine>? ResultLines { get; set; }
}
