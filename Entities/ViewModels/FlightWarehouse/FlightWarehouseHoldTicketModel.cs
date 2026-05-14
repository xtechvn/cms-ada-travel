using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.FlightWarehouse
{
    public class FlightWarehouseHoldTicketModel: FlightWarehouseHoldTicket
    {
        public string ExpirationDateStr { get; set; }
    }
}
