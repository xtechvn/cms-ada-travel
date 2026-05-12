using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.FlightWarehouse
{
    public class FlightWarehouseHoldTicketSearchModel
    {
        public long? FlightWarehouseBookingId { get; set; }
        public string GroupObject { get; set; }
        public string Status { get; set; }
        public int pageIndex { get; set; }
        public int pageSize { get; set; }
    }
}
