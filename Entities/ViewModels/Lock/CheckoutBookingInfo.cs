using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Lock
{
    public class CheckoutBookingInfo
    {
        public long BookingId { get; set; }
        public long HotelId { get; set; }
        public DateTime ArrivalDate { get; set; }
        public DateTime DepartureDate { get; set; }
        public string CheckoutTime { get; set; } // "14:00" nếu null
        public DateTime CheckoutDateTime { get; set; }
    }

}
