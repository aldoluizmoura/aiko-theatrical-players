using TheatricalPlayersRefactoringKata.Genre;
using Xunit;

namespace TheatricalPlayersRefactoringKata.Tests;

public class TragedyPricingTests
{
    private readonly TragedyPricing _pricing = new();

    [Fact]
    public void CalculateAmount_WhenAudienceIsAtMost30_ReturnsOnlyBaseAmount()
    {
        var result = _pricing.CalculateAmount(baseAmount: 40000, audience: 30);

        Assert.Equal(40000, result);
    }

    [Fact]
    public void CalculateAmount_WhenAudienceIsAbove30_AddsTenDollarsPerExtraSpectator()
    {
        // base 400.00 + 10.00 * (55 - 30) = 650.00 → 65000 cents
        var result = _pricing.CalculateAmount(baseAmount: 40000, audience: 55);

        Assert.Equal(65000, result);
    }

    [Fact]
    public void CalculateCredits_WhenAudienceIsAtMost30_ReturnsZero()
    {
        var result = _pricing.CalculateCredits(audience: 30);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateCredits_WhenAudienceIsAbove30_ReturnsOnePerExtraSpectator()
    {
        var result = _pricing.CalculateCredits(audience: 55);

        Assert.Equal(25, result);
    }
}
