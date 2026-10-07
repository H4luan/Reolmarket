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
        public int EmployeeId { get; set; }

        public decimal TotalAmount =>
            Lines.Sum(line => line.Quantity * line.SalePrice);

        public void AddProduct(Product product, int quantity)
        {
            if (quantity <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(quantity),
                    "Antallet skal være større end 0.");
            }

            if (quantity > product.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Der er ikke nok af produktet på lager.");
            }

            if (Lines.Any(line => line.ProductId == product.ProductId))
            {
                throw new InvalidOperationException(
                    "Produktet findes allerede i salget.");
            }

            Lines.Add(new SaleLine
            {
                ProductId = product.ProductId,
                ShelfId = product.ShelfId,
                Product = product,
                Quantity = quantity,
                SalePrice = product.Price
            });
        }
    }
}
