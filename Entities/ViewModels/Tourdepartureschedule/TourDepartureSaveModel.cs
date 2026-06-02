using Entities.Models;
using System.Collections.Generic;

namespace Entities.ViewModels.Tourdepartureschedule
{
    public class TourDepartureSaveModel : TourDeparture
    {
        public List<TourItinerary> TourItineraries { get; set; }
        public List<TourPrice> TourPrices { get; set; }
    }
}
