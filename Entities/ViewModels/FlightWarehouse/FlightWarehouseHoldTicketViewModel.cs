using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.FlightWarehouse
{
    public class FlightWarehouseHoldTicketViewModel: FlightWarehouseHoldTicket
    {
        public string StatusName { get; set; }
        public string GroupObjectName { get; set; }
        public string FullName { get; set; }
        public int TotalAmount { get; set; }
        public int Profit { get; set; }
        public int TotalRow { get; set; }
    }
}
