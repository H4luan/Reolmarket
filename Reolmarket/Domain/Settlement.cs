using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Reolmarket.Domain;

public class Settlement : INotifyPropertyChanged
{
    private decimal _grossSales;
    private decimal _totalReturns;
    private decimal _commission;
    private decimal _rentForNextPeriod;

    public int SettlementId { get; set; }
    public int TenantId { get; set; }
    public Tenant? Tenant { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal GrossSales { get => _grossSales; set { if (_grossSales != value) { _grossSales = value; OnAmountsChanged(); } } }
    public decimal TotalReturns { get => _totalReturns; set { if (_totalReturns != value) { _totalReturns = value; OnAmountsChanged(); } } }
    public decimal NetSales => GrossSales - TotalReturns;
    public decimal Commission { get => _commission; set { if (_commission != value) { _commission = value; OnAmountsChanged(); } } }
    public decimal RentForNextPeriod { get => _rentForNextPeriod; set { if (_rentForNextPeriod != value) { _rentForNextPeriod = value; OnAmountsChanged(); } } }
    public bool IsFinalized { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public int? ModifiedByEmployeeId { get; set; }
    public string ModifiedByEmployeeName { get; set; } = string.Empty;
    public decimal NetAmount => NetSales - Commission - RentForNextPeriod;

    public string ResultMessage => NetAmount >= 0
        ? $"Der skal udbetales {NetAmount:N2} kr. til lejeren."
        : $"Lejeren skal betale {Math.Abs(NetAmount):N2} kr.";

    public string HistoryResultMessage => NetAmount >= 0
        ? $"Afregningen viste, at der skulle udbetales {NetAmount:N2} kr. til lejeren."
        : $"Afregningen viste, at lejeren skulle betale {Math.Abs(NetAmount):N2} kr.";

    public string AuditMessage => LastModifiedAt is null
        ? "Afregningen er ikke rettet siden oprettelsen."
        : $"Rettet {LastModifiedAt:dd-MM-yyyy HH:mm} af {ModifiedByEmployeeName}.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void CalculateCommission()
    {
        Commission = NetSales * 0.10m;
    }

    private void OnAmountsChanged()
    {
        OnPropertyChanged(nameof(NetSales));
        OnPropertyChanged(nameof(NetAmount));
        OnPropertyChanged(nameof(ResultMessage));
        OnPropertyChanged(nameof(HistoryResultMessage));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
