using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using TheatricalPlayersRefactoringKata.Statement.Interfaces;
using StatementModel = TheatricalPlayersRefactoringKata.Statement.Statement;

namespace TheatricalPlayersRefactoringKata.Formatting;

public class XmlStatementFormatter : IStatementFormatter
{
    public string Format(StatementModel statement)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            OmitXmlDeclaration = false
        };

        using var stream = new MemoryStream();
        using (var writer = XmlWriter.Create(stream, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("Statement");
            writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");

            writer.WriteElementString("Customer", statement.Customer);

            writer.WriteStartElement("Items");
            foreach (var line in statement.Lines)
            {
                writer.WriteStartElement("Item");
                writer.WriteElementString("AmountOwed", FormatAmount(line.AmountInCents / 100m));
                writer.WriteElementString("EarnedCredits", line.Credits.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Seats", line.Audience.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteElementString("AmountOwed", FormatAmount(statement.TotalAmountInCents / 100m));
            writer.WriteElementString("EarnedCredits", statement.TotalCredits.ToString(CultureInfo.InvariantCulture));

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static string FormatAmount(decimal amount)
    {
        return amount.ToString("0.##", CultureInfo.InvariantCulture);
    }
}
