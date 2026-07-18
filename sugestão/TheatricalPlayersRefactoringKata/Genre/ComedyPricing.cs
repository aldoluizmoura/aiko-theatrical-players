using System;

namespace TheatricalPlayersRefactoringKata.Genre;

public class ComedyPricing : IGenrePricing
{
    public int CalculateAmount(int baseAmount, int audience)
    {
        var amount = baseAmount;

        if (audience > 20)
        {
            amount += 10000 + 500 * (audience - 20);
        }

        amount += 300 * audience;
        return amount;
    }

    public int CalculateCredits(int audience)
    {
        var credits = Math.Max(audience - 30, 0);
        credits += (int)Math.Floor((decimal)audience / 5);
        return credits;
    }
}
