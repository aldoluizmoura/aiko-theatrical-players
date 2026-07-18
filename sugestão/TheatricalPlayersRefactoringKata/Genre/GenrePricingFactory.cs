using System;

namespace TheatricalPlayersRefactoringKata.Genre;

public static class GenrePricingFactory
{
    public static IGenrePricing Create(string type)
    {
        return type switch
        {
            "tragedy" => new TragedyPricing(),
            "comedy" => new ComedyPricing(),
            "history" => new HistoryPricing(),
            _ => throw new Exception("unknown type: " + type)
        };
    }
}
