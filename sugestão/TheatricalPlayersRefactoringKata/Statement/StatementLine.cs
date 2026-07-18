namespace TheatricalPlayersRefactoringKata.Statement;

public class StatementLine
{
    public string Name { get; }
    public int AmountInCents { get; }
    public int Audience { get; }
    public int Credits { get; }

    public StatementLine(string name, int amountInCents, int audience, int credits)
    {
        Name = name;
        AmountInCents = amountInCents;
        Audience = audience;
        Credits = credits;
    }
}
