using System.Globalization;
using System.Text;
using StatementModel = TheatricalPlayersRefactoringKata.Statement.Statement;

namespace TheatricalPlayersRefactoringKata.Formatting;

public class TextStatementFormatter : IStatementFormatter
{
    public string Format(StatementModel statement)
    {
        var cultureInfo = new CultureInfo("en-US");
        var result = new StringBuilder();

        result.AppendFormat("Statement for {0}\n", statement.Customer);

        foreach (var line in statement.Lines)
        {
            result.AppendFormat(
                cultureInfo,
                "  {0}: {1:C} ({2} seats)\n",
                line.Name,
                line.AmountInCents / 100m,
                line.Audience);
        }

        result.AppendFormat(
            cultureInfo,
            "Amount owed is {0:C}\n",
            statement.TotalAmountInCents / 100m);
        result.AppendFormat("You earned {0} credits\n", statement.TotalCredits);

        return result.ToString();
    }
}
