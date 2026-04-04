using System.Text.RegularExpressions;

namespace ScanVul.Server.Infrastructure.Winget.Services;

public partial class WingetVersionComparer : IComparer<string>
{
    public static readonly WingetVersionComparer Instance = new();

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y)) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        // 1. Разбиваем версии на сегменты по разделителям
        // Используем '.', '-', '_', '+', и переход от букв к цифрам
        var partsX = SplitVersion(x);
        var partsY = SplitVersion(y);

        var length = Math.Max(partsX.Length, partsY.Length);

        for (var i = 0; i < length; i++)
        {
            // Если сегменты закончились, версия короче считается "меньшей" (обычно)
            // Пример: 1.0 vs 1.0.1 -> 1.0 меньше
            var pX = i < partsX.Length ? partsX[i] : null;
            var pY = i < partsY.Length ? partsY[i] : null;

            if (pX == null) return -1; // y длиннее -> y больше
            if (pY == null) return 1;  // x длиннее -> x больше

            // 2. Сравниваем сегменты
            int comparison;
            
            // Пытаемся распарсить как числа (BigInteger не нужен, long хватит с головой)
            var isNumX = long.TryParse(pX, out var numX);
            var isNumY = long.TryParse(pY, out var numY);

            if (isNumX && isNumY)
            {
                // Оба числа: сравниваем значения (10 > 2)
                comparison = numX.CompareTo(numY);
            }
            else if (!isNumX && !isNumY)
            {
                // Оба текст: лексикографическое сравнение (rc > beta)
                // Игнорируем регистр
                comparison = string.Compare(pX, pY, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                // Смешанный тип. Обычно числа считаются "больше" текста в контексте версий
                // 1.0.1 > 1.0.beta
                comparison = isNumX ? 1 : -1;
            }

            if (comparison != 0) return comparison;
        }

        return 0;
    }

    private static string[] SplitVersion(string version)
    {
        return SeparatorsRegex().Split(version)
            .Where(s => !string.IsNullOrEmpty(s))
            .ToArray();
    }

    [GeneratedRegex(@"[\.\-_\+\s]+|(?<=\d)(?=\D)|(?<=\D)(?=\d)")]
    private static partial Regex SeparatorsRegex();
}