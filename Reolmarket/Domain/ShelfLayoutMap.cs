namespace Reolmarket.Domain;

public static class ShelfLayoutMap
{
    public static bool AreAdjacent(int firstShelfNumber, int secondShelfNumber)
    {
        var first = GetPosition(firstShelfNumber);
        var second = GetPosition(secondShelfNumber);

        if (first is null || second is null)
        {
            return false;
        }

        return first.Value.Group == second.Value.Group &&
               Math.Abs(first.Value.Row - second.Value.Row) +
               Math.Abs(first.Value.Column - second.Value.Column) == 1;
    }

    private static (int Group, int Row, int Column)? GetPosition(
        int shelfNumber)
    {
        if (shelfNumber is >= 1 and <= 13)
        {
            return (1, shelfNumber - 1, 0);
        }

        if (shelfNumber is >= 14 and <= 18)
        {
            return (2, 0, shelfNumber - 14);
        }

        if (shelfNumber is >= 19 and <= 24)
        {
            int index = shelfNumber - 19;
            return (3, index / 3, index % 3);
        }

        if (shelfNumber is >= 25 and <= 38)
        {
            int index = shelfNumber - 25;
            return (4, index / 7, index % 7);
        }

        if (shelfNumber is >= 39 and <= 52)
        {
            int index = shelfNumber - 39;
            return (5, index / 7, index % 7);
        }

        if (shelfNumber is >= 53 and <= 66)
        {
            int index = shelfNumber - 53;
            return (6, index / 7, index % 7);
        }

        if (shelfNumber is >= 67 and <= 76)
        {
            int index = shelfNumber - 67;
            return (7, index / 5, index % 5);
        }

        if (shelfNumber is 77 or 78)
        {
            return (8, 0, shelfNumber - 77);
        }

        if (shelfNumber is 79 or 80)
        {
            return (9, 0, shelfNumber - 79);
        }

        return null;
    }
}