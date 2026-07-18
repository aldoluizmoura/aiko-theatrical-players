using System;
using System.Collections.Generic;
using System.Globalization;

namespace TheatricalPlayersRefactoringKata;

public class StatementPrinter
{
    private readonly PerformanceCalculator _calculator = new();

    public string Print(Invoice invoice, Dictionary<string, Play> plays)
    {
        var totalAmount = 0;
        var volumeCredits = 0;
        var result = string.Format("Statement for {0}\n", invoice.Customer);
        CultureInfo cultureInfo = new CultureInfo("en-US");

        foreach (var perf in invoice.Performances)
        {
            var play = plays[perf.PlayId];
            var thisAmount = _calculator.CalculateAmount(play, perf);
            volumeCredits += _calculator.CalculateCredits(play, perf);

            result += String.Format(cultureInfo, "  {0}: {1:C} ({2} seats)\n", play.Name, thisAmount / 100m, perf.Audience);
            totalAmount += thisAmount;
        }
        result += String.Format(cultureInfo, "Amount owed is {0:C}\n", totalAmount / 100m);
        result += String.Format("You earned {0} credits\n", volumeCredits);
        return result;
    }
}
