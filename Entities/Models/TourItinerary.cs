using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TourItinerary
    {
        public long Id { get; set; }
        public long TourDepartureId { get; set; }
        public int RouteType { get; set; }
        public int TransportType { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public string TransportProvider { get; set; }
        public DateTime? DepartureDate { get; set; }
        public string TransportCode { get; set; }
        public string Note { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
