using System;

namespace Reolmarket.Domain;

public class ReturnableSaleLine
{
    public int SaleLineId { get; set; }
    public int SaleId { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime SaleDate { get; set; }
    public int SoldQuantity { get; set; }
    public int ReturnedQuantity { get; set; }
    public int RemainingQuantity => SoldQuantity - ReturnedQuantity;
    public decimal SalePrice { get; set; }

    public string DisplayText =>
        $"Salg #{SaleId}: {ProductName}, {SaleDate:dd-MM-yyyy}, " +
        $"{RemainingQuantity} tilbage à {SalePrice:N2} kr.";
}