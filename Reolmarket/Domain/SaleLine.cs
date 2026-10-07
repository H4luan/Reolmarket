namespace Reolmarket.Domain;

public class SaleLine
{
    public int SaleLineId { get; set; }

    public int SaleId { get; set; }

    public Sale? Sale { get; set; }

    public int ProductId { get; set; }

    public int ShelfId { get; set; }

    public string? Comment { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; }

    public decimal SalePrice { get; set; }
}