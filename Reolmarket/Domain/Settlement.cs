using System;

namespace Reolmarket.Domain
{
    public class Settlement
    {
        public int SettlementId { get; set; }

        public int TenantId { get; set; }

        public Tenant? Tenant { get; set; }

        public DateTime PeriodStart { get; set; }

        public DateTime PeriodEnd { get; set; }

        public decimal GrossSales { get; set; }

        public decimal Commission { get; set; }

        public decimal RentForNextPeriod { get; set; }

        public bool IsFinalized { get; set; }

        public decimal NetAmount =>
            GrossSales - Commission - RentForNextPeriod;

        public void CalculateCommission()
        {
            Commission = GrossSales * 0.10m;
        }
    }
}
