using System;

namespace Reolmarket.Domain
{
    public class Item
    {
        public int ItemId { get; set; }

        public int RentalId { get; set; }

        public Rental? Rental { get; set; }

        public string Barcode { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public SaleLine? SaleLine { get; set; }

        public bool IsSold => SaleLine is not null;
    }
}
