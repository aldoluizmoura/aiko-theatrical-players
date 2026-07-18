using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Formatting;
using TheatricalPlayersRefactoringKata.Statement;
using TheatricalPlayersRefactoringKata.Statement.Interfaces;

namespace TheatricalPlayersRefactoringKata;

public class StatementPrinter
{
    private readonly StatementGenerator _generator = new();
    private readonly IStatementFormatter _formatter = new TextStatementFormatter();

    public string Print(Invoice invoice, Dictionary<string, Play> plays)
    {
        var statement = _generator.Generate(invoice, plays);
        return _formatter.Format(statement);
    }
}
