namespace TheatricalPlayersRefactoringKata.Genre;

public interface IGenrePricing
{
    int CalculateAmount(int baseAmount, int audience);
    int CalculateCredits(int audience);
}
