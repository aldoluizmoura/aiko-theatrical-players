using System.Collections.Generic;

namespace TheatricalPlayersRefactoringKata.Statement;

public class Statement
{
    public string Customer { get; }
    public IReadOnlyList<StatementLine> Lines { get; }
    public int TotalAmountInCents { get; }
    public int TotalCredits { get; }

    public Statement(
        string customer,
        IReadOnlyList<StatementLine> lines,
        int totalAmountInCents,
        int totalCredits)
    {
        Customer = customer;
        Lines = lines;
        TotalAmountInCents = totalAmountInCents;
        TotalCredits = totalCredits;
    }
}
