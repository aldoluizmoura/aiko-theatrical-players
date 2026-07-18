using StatementModel = TheatricalPlayersRefactoringKata.Statement.Statement;

namespace TheatricalPlayersRefactoringKata.Statement.Interfaces;

public interface IStatementFormatter
{
    string Format(StatementModel statement);
}
