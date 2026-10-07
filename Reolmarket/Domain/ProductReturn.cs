using System;

namespace Reolmarket.Domain;

public class ProductReturn
{
    public int ProductReturnId { get; set; }
    public DateTime ReturnDate { get; set; }
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public string? Comment { get; set; }
    public int SaleLineId { get; set; }
    public SaleLine? SaleLine { get; set; }
}