using System.Collections.Generic;

namespace Reolmarket.Domain
{
    public class Tenant
    {
        public int TenantId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<Rental> Rentals { get; set; } = new();

        public List<Settlement> Settlements { get; set; } = new();
    }
}
