using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class FlightWarehousePrice
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public int? PriceType { get; set; }
        public string ApplyAgencyIds { get; set; }
        public decimal? AdtPrice { get; set; }
        public decimal? AdtAmount { get; set; }
        public decimal? AdtProfit { get; set; }
        public decimal? ChdPrice { get; set; }
        public decimal? ChdAmount { get; set; }
        public decimal? ChdProfit { get; set; }
        public decimal? InfPrice { get; set; }
        public decimal? InfAmount { get; set; }
        public decimal? InfProfit { get; set; }
    }
}
