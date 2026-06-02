using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Tourdepartureschedule
{
    public class GetListTourDepartureModel : TourDeparture
    {
        public string Statusname { get; set; }
        public string TourProductName { get; set; }
        public string CreatedName { get; set; }
        public string UpdatedName { get; set; }
        public int TotalRow { get; set; }
        public double adtAmount { get; set; }
        public string EndPoint { get; set; }
        public string StartPoint { get; set; }

    }
}
