using System;
using System.Collections.Generic;

namespace Reolmarket.Domain
{
    public class Shelf
    {
        public int ShelfId { get; set; }

        public int ShelfNumber { get; set; }

        public ShelfLayout Layout { get; set; }

        public List<Rental> Rentals { get; set; } = new();

        public bool IsAvailable(DateTime date)
        {
            return !Rentals.Any(r =>
                r.StartDate <= date &&
                (r.EndDate == null || r.EndDate >= date));
        }
    }
}