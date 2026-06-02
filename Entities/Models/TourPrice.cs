using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TourPrice
    {
        public long Id { get; set; }
        public long TourDepartureId { get; set; }
        public int PriceType { get; set; }
        public int? AdultQuantity { get; set; }
        public int? ChildQuantity { get; set; }
        public int? InfantQuantity { get; set; }
        public decimal? AdtPrice { get; set; }
        public decimal? AdtAmount { get; set; }
        public decimal? AdtProfit { get; set; }
        public decimal? ChdPrice { get; set; }
        public decimal? ChdAmount { get; set; }
        public decimal? ChdProfit { get; set; }
        public decimal? InfPrice { get; set; }
        public decimal? InfAmount { get; set; }
        public decimal? InfProfit { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
