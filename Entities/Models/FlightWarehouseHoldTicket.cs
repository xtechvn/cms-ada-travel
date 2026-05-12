using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class FlightWarehouseHoldTicket
    {
        public long Id { get; set; }
        public long FlightWarehouseBookingId { get; set; }
        public int GroupObject { get; set; }
        public int TotalTicket { get; set; }
        public int Status { get; set; }
        public int AdultQuantity { get; set; }
        public int ChildQuantity { get; set; }
        public int InfantQuantity { get; set; }
        public long AdultAmount { get; set; }
        public long AdultPrice { get; set; }
        public long ChildAmount { get; set; }
        public long ChildPrice { get; set; }
        public long InfantAmount { get; set; }
        public long InfantPrice { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
