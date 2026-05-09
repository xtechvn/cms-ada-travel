using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class FlightWarehouseSegment
    {
        public long Id { get; set; }
        public long BookingId { get; set; }
        public int? SegmentType { get; set; }
        public DateTime? FlightDate { get; set; }
        public string Airline { get; set; }
        public string FlightCode { get; set; }
        public string Pnrcode { get; set; }
    }
}
