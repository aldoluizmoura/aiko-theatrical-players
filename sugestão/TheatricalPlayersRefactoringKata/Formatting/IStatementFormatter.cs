using StatementModel = TheatricalPlayersRefactoringKata.Statement.Statement;

namespace TheatricalPlayersRefactoringKata.Formatting;

public interface IStatementFormatter
{
    string Format(StatementModel statement);
}
