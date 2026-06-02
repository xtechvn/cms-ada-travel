using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TourDeparture
    {
        public long Id { get; set; }
        public long TourProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Total { get; set; }
        public DateTime? BookingDeadline { get; set; }
        public int? Status { get; set; }
        public string Note { get; set; }
        public bool? IsShowWebsite { get; set; }
        public bool? IsShowWaitingList { get; set; }
        public bool? IsAllowReserveOnline { get; set; }
        public bool? IsAllowDepositOnline { get; set; }
        public bool? IsFeatured { get; set; }
        public long? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public long? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
