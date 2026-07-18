using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Statement;
using Xunit;

namespace TheatricalPlayersRefactoringKata.Tests;

public class StatementGeneratorTests
{
    private readonly StatementGenerator _generator = new();

    [Fact]
    public void Generate_BuildsStatementWithCustomerLinesAndTotals()
    {
        var plays = new Dictionary<string, Play>
        {
            ["hamlet"] = new Play("Hamlet", 4024, "tragedy"),
            ["as-like"] = new Play("As You Like It", 2670, "comedy")
        };

        var invoice = new Invoice(
            "BigCo",
            new List<Performance>
            {
                new Performance("hamlet", 55),
                new Performance("as-like", 35)
            });

        var statement = _generator.Generate(invoice, plays);

        Assert.Equal("BigCo", statement.Customer);
        Assert.Equal(2, statement.Lines.Count);

        Assert.Equal("Hamlet", statement.Lines[0].Name);
        Assert.Equal(65000, statement.Lines[0].AmountInCents);
        Assert.Equal(55, statement.Lines[0].Audience);
        Assert.Equal(25, statement.Lines[0].Credits);

        Assert.Equal("As You Like It", statement.Lines[1].Name);
        Assert.Equal(54700, statement.Lines[1].AmountInCents);
        Assert.Equal(35, statement.Lines[1].Audience);
        Assert.Equal(12, statement.Lines[1].Credits);

        Assert.Equal(119700, statement.TotalAmountInCents);
        Assert.Equal(37, statement.TotalCredits);
    }

    [Fact]
    public void Generate_WithHistoryPlay_UsesHistoryPricingWithoutComedyCreditBonus()
    {
        var plays = new Dictionary<string, Play>
        {
            ["henry-v"] = new Play("Henry V", 3227, "history")
        };

        var invoice = new Invoice(
            "BigCo",
            new List<Performance>
            {
                new Performance("henry-v", 20)
            });

        var statement = _generator.Generate(invoice, plays);

        Assert.Single(statement.Lines);
        Assert.Equal(70540, statement.Lines[0].AmountInCents);
        Assert.Equal(0, statement.Lines[0].Credits);
        Assert.Equal(70540, statement.TotalAmountInCents);
        Assert.Equal(0, statement.TotalCredits);
    }
}
