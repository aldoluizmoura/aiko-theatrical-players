using TheatricalPlayersRefactoringKata.Genre;
using Xunit;

namespace TheatricalPlayersRefactoringKata.Tests;

public class ComedyPricingTests
{
    private readonly ComedyPricing _pricing = new();

    [Fact]
    public void CalculateAmount_WhenAudienceIsAtMost20_AddsThreeDollarsPerSpectator()
    {
        // base 200.00 + 3.00 * 20 = 260.00 → 26000 cents
        var result = _pricing.CalculateAmount(baseAmount: 20000, audience: 20);

        Assert.Equal(26000, result);
    }

    [Fact]
    public void CalculateAmount_WhenAudienceIsAbove20_AddsBonusAndFiveDollarsPerExtraSpectator()
    {
        // base 267.00 + 100.00 + 5.00 * (35 - 20) + 3.00 * 35 = 547.00 → 54700 cents
        var result = _pricing.CalculateAmount(baseAmount: 26700, audience: 35);

        Assert.Equal(54700, result);
    }

    [Fact]
    public void CalculateCredits_WhenAudienceIsAtMost30_ReturnsOnlyComedyBonus()
    {
        // floor(20 / 5) = 4
        var result = _pricing.CalculateCredits(audience: 20);

        Assert.Equal(4, result);
    }

    [Fact]
    public void CalculateCredits_WhenAudienceIsAbove30_AddsBaseCreditsAndComedyBonus()
    {
        // (35 - 30) + floor(35 / 5) = 5 + 7 = 12
        var result = _pricing.CalculateCredits(audience: 35);

        Assert.Equal(12, result);
    }
}
