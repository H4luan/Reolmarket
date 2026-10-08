namespace Reolmarket.Domain;

public class AvailableShelfInfo
{
    public int ShelfNumber { get; set; }
    public string Comment { get; set; } = string.Empty;
    public string LayoutDescription { get; set; } = string.Empty;
    public decimal MonthlyRentForOne { get; set; } = 850m;
}
