using System;
using TheatricalPlayersRefactoringKata.Genre.Interfaces;

namespace TheatricalPlayersRefactoringKata.Genre;

public class TragedyPricing : IGenrePricing
{
    public int CalculateAmount(int baseAmount, int audience)
    {
        var amount = baseAmount;

        if (audience > 30)
        {
            amount += 1000 * (audience - 30);
        }

        return amount;
    }

    public int CalculateCredits(int audience)
    {
        return Math.Max(audience - 30, 0);
    }
}
