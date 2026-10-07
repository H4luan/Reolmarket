using System;
using System.Collections.Generic;

namespace Reolmarket.Domain
{
    public class Rental
    {
        public int RentalId { get; set; }

        public int TenantId { get; set; }

        public Tenant? Tenant { get; set; }

        public int ShelfId { get; set; }

        public Shelf? Shelf { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public decimal MonthlyRent { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public List<Item> Items { get; set; } = new();

        public bool IsActive(DateTime date)
        {
            return StartDate <= date &&
                   (EndDate == null || EndDate >= date);
        }
    }
}
