using TheatricalPlayersRefactoringKata.Api.Processing;

namespace TheatricalPlayersRefactoringKata.Api.Persistence;

public class StatementEntity
{
    public Guid Id { get; set; }
    public string Customer { get; set; } = string.Empty;
    public StatementJobStatus Status { get; set; } = StatementJobStatus.Queued;
    public string? OutputFilePath { get; set; }
    public string? XmlContent { get; set; }
    public string? Error { get; set; }
    public int? TotalAmountInCents { get; set; }
    public int? TotalCredits { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    public List<StatementPlayEntity> Plays { get; set; } = [];
    public List<StatementPerformanceEntity> Performances { get; set; } = [];
    public List<StatementLineEntity> Lines { get; set; } = [];
}

public class StatementPlayEntity
{
    public int Id { get; set; }
    public Guid StatementId { get; set; }
    public StatementEntity Statement { get; set; } = null!;
    public string PlayKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int LinesCount { get; set; }
    public string Type { get; set; } = string.Empty;
}

public class StatementPerformanceEntity
{
    public int Id { get; set; }
    public Guid StatementId { get; set; }
    public StatementEntity Statement { get; set; } = null!;
    public string PlayKey { get; set; } = string.Empty;
    public int Audience { get; set; }
}

public class StatementLineEntity
{
    public int Id { get; set; }
    public Guid StatementId { get; set; }
    public StatementEntity Statement { get; set; } = null!;
    public string PlayName { get; set; } = string.Empty;
    public int AmountInCents { get; set; }
    public int Audience { get; set; }
    public int Credits { get; set; }
}
