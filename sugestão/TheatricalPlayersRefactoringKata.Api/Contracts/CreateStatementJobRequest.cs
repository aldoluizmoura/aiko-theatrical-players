namespace TheatricalPlayersRefactoringKata.Api.Contracts;

public class CreateStatementJobRequest
{
    public required string Customer { get; set; }
    public required Dictionary<string, PlayRequest> Plays { get; set; }
    public required List<PerformanceRequest> Performances { get; set; }
}

public class PlayRequest
{
    public required string Name { get; set; }
    public int Lines { get; set; }
    public required string Type { get; set; }
}

public class PerformanceRequest
{
    public required string PlayId { get; set; }
    public int Audience { get; set; }
}
