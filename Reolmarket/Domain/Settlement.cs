using System;

namespace Reolmarket.Domain;

public class Settlement
{
    public int SettlementId { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal GrossSales { get; set; }
    public decimal TotalReturns { get; set; }
    public decimal NetSales => GrossSales - TotalReturns;
    public decimal Commission { get; set; }
    public decimal RentForNextPeriod { get; set; }
    public bool IsFinalized { get; set; }
    public decimal NetAmount => NetSales - Commission - RentForNextPeriod;

    public string ResultMessage => NetAmount >= 0
        ? $"Der skal udbetales {NetAmount:N2} kr. til lejeren."
        : $"Lejeren skal betale {Math.Abs(NetAmount):N2} kr.";

    public void CalculateCommission()
    {
        Commission = NetSales * 0.10m;
    }
}