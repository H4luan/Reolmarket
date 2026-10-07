namespace Reolmarket.Domain;

public class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string Barcode { get; set; } = string.Empty;

    public int StockQuantity { get; set; }

    public int ShelfId { get; set; }

    public Shelf? Shelf { get; set; }
}