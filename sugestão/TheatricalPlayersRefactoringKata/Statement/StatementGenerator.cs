using System.Collections.Generic;
using TheatricalPlayersRefactoringKata.Genre;

namespace TheatricalPlayersRefactoringKata.Statement;

public class StatementGenerator
{
    public Statement Generate(Invoice invoice, Dictionary<string, Play> plays)
    {
        var lines = new List<StatementLine>();
        var totalAmount = 0;
        var totalCredits = 0;

        foreach (var performance in invoice.Performances)
        {
            var play = plays[performance.PlayId];
            var genre = GenrePricingFactory.Create(play.Type);
            var baseAmount = BaseAmountCalculator.Calculate(play.Lines);
            var amount = genre.CalculateAmount(baseAmount, performance.Audience);
            var credits = genre.CalculateCredits(performance.Audience);

            lines.Add(new StatementLine(play.Name, amount, performance.Audience, credits));
            totalAmount += amount;
            totalCredits += credits;
        }

        return new Statement(invoice.Customer, lines, totalAmount, totalCredits);
    }
}
