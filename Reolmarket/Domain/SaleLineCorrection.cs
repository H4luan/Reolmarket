namespace Reolmarket.Domain;

public class SaleLineCorrection
{
    public int SaleLineId { get; set; }
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal SalePrice { get; set; }
    public int ShelfId { get; set; }
    public int ShelfNumber { get; set; }
    public string? Comment { get; set; }
}
