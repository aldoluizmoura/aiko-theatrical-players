using System;
using TheatricalPlayersRefactoringKata.Genre.Interfaces;

namespace TheatricalPlayersRefactoringKata.Genre;

public class HistoryPricing : IGenrePricing
{
    private readonly TragedyPricing _tragedy = new();
    private readonly ComedyPricing _comedy = new();

    public int CalculateAmount(int baseAmount, int audience)
    {
        return _tragedy.CalculateAmount(baseAmount, audience)
             + _comedy.CalculateAmount(baseAmount, audience);
    }

    public int CalculateCredits(int audience)
    {
        return Math.Max(audience - 30, 0);
    }
}
