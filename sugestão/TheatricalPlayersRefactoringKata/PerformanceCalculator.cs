using System;

namespace TheatricalPlayersRefactoringKata;

public class PerformanceCalculator
{
    public int CalculateAmount(Play play, Performance performance)
    {
        var baseAmount = CalculateBaseAmount(play.Lines);

        return play.Type switch
        {
            "tragedy" => CalculateTragedyAmount(baseAmount, performance.Audience),
            "comedy" => CalculateComedyAmount(baseAmount, performance.Audience),
            _ => throw new Exception("unknown type: " + play.Type)
        };
    }

    public int CalculateCredits(Play play, Performance performance)
    {
        var credits = Math.Max(performance.Audience - 30, 0);

        if (play.Type == "comedy")
        {
            credits += (int)Math.Floor((decimal)performance.Audience / 5);
        }

        return credits;
    }

    private static int CalculateBaseAmount(int lines)
    {
        var clampedLines = Math.Clamp(lines, 1000, 4000);
        return clampedLines * 10;
    }

    private static int CalculateTragedyAmount(int baseAmount, int audience)
    {
        var amount = baseAmount;

        if (audience > 30)
        {
            amount += 1000 * (audience - 30);
        }

        return amount;
    }

    private static int CalculateComedyAmount(int baseAmount, int audience)
    {
        var amount = baseAmount;

        if (audience > 20)
        {
            amount += 10000 + 500 * (audience - 20);
        }

        amount += 300 * audience;
        return amount;
    }
}
