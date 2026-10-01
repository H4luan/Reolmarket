using System;
using System.Collections.Generic;
using System.Linq;

namespace Reolmarket.Domain
{
    public class Shelf
    {
        public int ShelfId { get; set; }

        public int ShelfNumber { get; set; }

        public ShelfLayout Layout { get; set; }

        public string Location { get; set; } = string.Empty;

        public List<Rental> Rentals { get; set; } = new();

        public bool IsAvailable(DateTime date)
        {
            return !Rentals.Any(r =>
                r.StartDate <= date &&
                (r.EndDate == null || r.EndDate >= date));
        }

        public bool IsAvailableNow => IsAvailable(DateTime.Today);
    }
}