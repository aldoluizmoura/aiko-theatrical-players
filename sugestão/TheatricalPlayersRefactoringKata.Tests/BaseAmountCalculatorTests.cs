using TheatricalPlayersRefactoringKata.Genre;
using Xunit;

namespace TheatricalPlayersRefactoringKata.Tests;

public class BaseAmountCalculatorTests
{
    [Fact]
    public void Calculate_WhenLinesAreWithinRange_ReturnsLinesTimesTen()
    {
        // 2670 lines → $267.00 → 26700 cents
        var result = BaseAmountCalculator.Calculate(2670);

        Assert.Equal(26700, result);
    }

    [Fact]
    public void Calculate_WhenLinesAreBelowMinimum_ClampsTo1000()
    {
        var result = BaseAmountCalculator.Calculate(500);

        Assert.Equal(10000, result);
    }

    [Fact]
    public void Calculate_WhenLinesAreAboveMaximum_ClampsTo4000()
    {
        // Hamlet: 4024 lines → clamped to 4000 → 40000 cents
        var result = BaseAmountCalculator.Calculate(4024);

        Assert.Equal(40000, result);
    }

    [Theory]
    [InlineData(1000, 10000)]
    [InlineData(4000, 40000)]
    public void Calculate_WhenLinesAreAtBoundaries_DoesNotClampFurther(int lines, int expected)
    {
        var result = BaseAmountCalculator.Calculate(lines);

        Assert.Equal(expected, result);
    }
}
