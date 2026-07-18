using System;
using TheatricalPlayersRefactoringKata.Genre;
using Xunit;

namespace TheatricalPlayersRefactoringKata.Tests;

public class GenrePricingFactoryTests
{
    [Theory]
    [InlineData("tragedy", typeof(TragedyPricing))]
    [InlineData("comedy", typeof(ComedyPricing))]
    [InlineData("history", typeof(HistoryPricing))]
    public void Create_WithKnownType_ReturnsMatchingPricing(string type, Type expected)
    {
        var pricing = GenrePricingFactory.Create(type);

        Assert.IsType(expected, pricing);
    }

    [Fact]
    public void Create_WithUnknownType_ThrowsWithDescriptiveMessage()
    {
        var exception = Assert.Throws<Exception>(() => GenrePricingFactory.Create("musical"));

        Assert.Equal("unknown type: musical", exception.Message);
    }
}
