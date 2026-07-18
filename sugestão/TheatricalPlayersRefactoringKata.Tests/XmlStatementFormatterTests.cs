using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Formatting;
using TheatricalPlayersRefactoringKata.Statement;
using Xunit;
using StatementModel = TheatricalPlayersRefactoringKata.Statement.Statement;

namespace TheatricalPlayersRefactoringKata.Tests;

public class XmlStatementFormatterTests
{
    private readonly XmlStatementFormatter _formatter = new();

    [Fact]
    public void Format_RendersExpectedXmlStructure()
    {
        var statement = new StatementModel(
            "BigCo",
            new List<StatementLine>
            {
                new StatementLine("Hamlet", 65000, 55, 25),
                new StatementLine("Henry V", 70540, 20, 0)
            },
            totalAmountInCents: 135540,
            totalCredits: 25);

        var result = _formatter.Format(statement);

        Assert.StartsWith("<?xml version=\"1.0\" encoding=\"utf-8\"?>", result);
        Assert.Contains("<Customer>BigCo</Customer>", result);
        Assert.Contains("<AmountOwed>650</AmountOwed>", result);
        Assert.Contains("<EarnedCredits>25</EarnedCredits>", result);
        Assert.Contains("<Seats>55</Seats>", result);
        Assert.Contains("<AmountOwed>705.4</AmountOwed>", result);
        Assert.Contains("<EarnedCredits>0</EarnedCredits>", result);
        Assert.Contains("<Seats>20</Seats>", result);
        Assert.Contains("<AmountOwed>1355.4</AmountOwed>", result);
        Assert.Contains("<EarnedCredits>25</EarnedCredits>", result);
    }

    [Fact]
    public void Format_WholeDollarAmounts_OmitTrailingDecimal()
    {
        var statement = new StatementModel(
            "BigCo",
            new List<StatementLine>
            {
                new StatementLine("Hamlet", 65000, 55, 25)
            },
            totalAmountInCents: 65000,
            totalCredits: 25);

        var result = _formatter.Format(statement);

        Assert.Contains("<AmountOwed>650</AmountOwed>", result);
        Assert.DoesNotContain("<AmountOwed>650.0</AmountOwed>", result);
    }
}
