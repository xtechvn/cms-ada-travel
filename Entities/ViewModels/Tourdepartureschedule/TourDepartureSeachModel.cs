using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.ViewModels.Tourdepartureschedule
{
    public class TourDepartureSeachModel
    {
        public int? TourProductId { get; set; }
        public int? Status { get; set; }
        public string FromDateStr { get; set; }
        public DateTime? FromDate
        {
            get
            {
                return DateUtil.StringToDate(FromDateStr);
            }
        }
      
        public string ToDateStr { get; set; }
        public DateTime? ToDate
        {
            get
            {
                return DateUtil.StringToDate(ToDateStr);
            }
        }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
