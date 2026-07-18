using TheatricalPlayersRefactoringKata.Genre;
using Xunit;

namespace TheatricalPlayersRefactoringKata.Tests;

public class HistoryPricingTests
{
    private readonly HistoryPricing _pricing = new();
    private readonly TragedyPricing _tragedy = new();
    private readonly ComedyPricing _comedy = new();

    [Fact]
    public void CalculateAmount_IsSumOfTragedyAndComedyAmounts()
    {
        const int baseAmount = 32270;
        const int audience = 20;

        var result = _pricing.CalculateAmount(baseAmount, audience);

        var expected =
            _tragedy.CalculateAmount(baseAmount, audience)
          + _comedy.CalculateAmount(baseAmount, audience);

        Assert.Equal(expected, result);
        Assert.Equal(70540, result); // Henry V: $705.40
    }

    [Fact]
    public void CalculateAmount_WithAudienceAboveThresholds_StillSumsBothGenres()
    {
        const int baseAmount = 26480;
        const int audience = 39;

        var result = _pricing.CalculateAmount(baseAmount, audience);

        Assert.Equal(93160, result); // King John: $931.60
    }

    [Fact]
    public void CalculateCredits_WhenAudienceIsAtMost30_ReturnsZero()
    {
        var result = _pricing.CalculateCredits(audience: 20);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateCredits_WhenAudienceIsAbove30_ReturnsOnlyBaseCreditsWithoutComedyBonus()
    {
        // history does not get comedy's floor(audience / 5) bonus
        var result = _pricing.CalculateCredits(audience: 39);

        Assert.Equal(9, result);
        Assert.NotEqual(_comedy.CalculateCredits(39), result);
    }
}
