using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Formatting;
using TheatricalPlayersRefactoringKata.Statement;
using TheatricalPlayersRefactoringKata.Statement.Interfaces;

namespace TheatricalPlayersRefactoringKata;

public class StatementPrinter
{
    private readonly StatementGenerator _generator = new();
    private readonly IStatementFormatter _textFormatter = new TextStatementFormatter();
    private readonly IStatementFormatter _xmlFormatter = new XmlStatementFormatter();

    public string Print(Invoice invoice, Dictionary<string, Play> plays)
    {
        var statement = _generator.Generate(invoice, plays);
        return _textFormatter.Format(statement);
    }

    public string PrintXml(Invoice invoice, Dictionary<string, Play> plays)
    {
        var statement = _generator.Generate(invoice, plays);
        return _xmlFormatter.Format(statement);
    }
}
