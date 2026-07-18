using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Formatting;
using TheatricalPlayersRefactoringKata.Statement;
using Xunit;
using StatementModel = TheatricalPlayersRefactoringKata.Statement.Statement;

namespace TheatricalPlayersRefactoringKata.Tests;

public class TextStatementFormatterTests
{
    private readonly TextStatementFormatter _formatter = new();

    [Fact]
    public void Format_RendersCustomerLinesTotalsAndCredits()
    {
        var statement = new StatementModel(
            "BigCo",
            new List<StatementLine>
            {
                new StatementLine("Hamlet", 65000, 55, 25),
                new StatementLine("As You Like It", 54700, 35, 12)
            },
            totalAmountInCents: 119700,
            totalCredits: 37);

        var result = _formatter.Format(statement);

        var expected =
            "Statement for BigCo\n" +
            "  Hamlet: $650.00 (55 seats)\n" +
            "  As You Like It: $547.00 (35 seats)\n" +
            "Amount owed is $1,197.00\n" +
            "You earned 37 credits\n";

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Format_PreservesCentsWhenAmountIsNotWholeDollars()
    {
        var statement = new StatementModel(
            "BigCo",
            new List<StatementLine>
            {
                new StatementLine("Henry V", 70540, 20, 0)
            },
            totalAmountInCents: 70540,
            totalCredits: 0);

        var result = _formatter.Format(statement);

        Assert.Contains("Henry V: $705.40 (20 seats)", result);
        Assert.Contains("Amount owed is $705.40", result);
    }
}
