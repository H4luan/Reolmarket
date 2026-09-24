using System;
using System.Collections.Generic;

namespace Reolmarket.Domain
{
    public class Sale
    {
        public int SaleId { get; set; }

        public DateTime SoldAt { get; set; }

        public PaymentMethod PaymentMethod { get; set; }

        public List<SaleLine> Lines { get; set; } = new();

        public decimal TotalAmount => Lines.Sum(line => line.PriceAtSale);

        public void AddItem(Item item)
        {
            if (item.IsSold)
            {
                throw new InvalidOperationException(
                    "Varen er allerede solgt og kan ikke tilføjes til salget.");
            }

            bool alreadyInCart = Lines.Any(line => line.ItemId == item.ItemId);

            if (alreadyInCart)
            {
                throw new InvalidOperationException(
                    "Varen findes allerede i kundens kurv.");
            }

            SaleLine saleLine = new SaleLine
            {
                ItemId = item.ItemId,
                Item = item,
                PriceAtSale = item.Price
            };

            Lines.Add(saleLine);
        }
    }
}
