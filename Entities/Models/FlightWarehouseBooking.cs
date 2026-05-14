using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class FlightWarehouseBooking
    {
        public long Id { get; set; }
        public string BookingCode { get; set; }
        public int? TripType { get; set; }
        public string DeparturePoint { get; set; }
        public string ArrivalPoint { get; set; }
        public int? TotalTicket { get; set; }
        public int? IsRefundable { get; set; }
        public string CarryOnBaggage { get; set; }
        public string CheckedBaggage { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? AgencyTotalTicket { get; set; }
    }
}
