using System;

namespace TheatricalPlayersRefactoringKata.Genre;

public static class BaseAmountCalculator
{
    public static int Calculate(int lines)
    {
        var clampedLines = Math.Clamp(lines, 1000, 4000);
        return clampedLines * 10;
    }
}
