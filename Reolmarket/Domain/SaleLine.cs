using System;

namespace Reolmarket.Domain
{
    public class SaleLine
    {
        public int SaleLineId { get; set; }

        public int SaleId { get; set; }

        public Sale? Sale { get; set; }

        public int ItemId { get; set; }

        public Item? Item { get; set; }

        public decimal PriceAtSale { get; set; }
    }
}
