using Entities.Models;
using System.Collections.Generic;

namespace Entities.ViewModels.FlightWarehouse
{
    public class FlightWarehouseUpsertModel
    {
        public FlightWarehouseBookingModel Booking { get; set; }
        public List<FlightWarehouseSegmentModel> Segments { get; set; }
        public List<FlightWarehousePriceModel> Prices { get; set; }
    }
    public class FlightWarehouseBookingModel : FlightWarehouseBooking
    {
       
    }
    public class FlightWarehouseSegmentModel : FlightWarehouseSegment
    {
      
    }
    public class FlightWarehousePriceModel : FlightWarehousePrice
    {
      
    }
}
