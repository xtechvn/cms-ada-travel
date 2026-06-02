using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Tourdepartureschedule
{
    public class DetailTourDepartureModel : TourDeparture
    {
        public string Statusname { get; set; }
        public string TourProductName { get; set; }
        public string CreatedName { get; set; }
        public string UpdatedName { get; set; }
        public List<TourItinerary> TourItineraries { get; set; }
        public List<TourPrice> TourPrices { get; set; }
    }
}
