using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.ViewModels.Tour
{
    public class OptionalViewModel
    {
        public int Id { get; set; }
        public int ServiceId { get; set; }
        public string ServiceCode { get; set; }
        public string PackageName { get; set; }
        public string ServiceName { get; set; }
        public double TotalAmountPay { get; set; }
        public double Amount { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int TotalRow { get; set; }
    } 
    public class HotelBookingRoomsOptionalModel: OptionalViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public short? NumberOfRooms { get; set; }
        public int Night { get; set; }
    } 
    public class FlyBookingPackagesOptionalModel : OptionalViewModel    
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string StartPoint { get; set; }
        public string EndPoint { get; set; }
        public string GroupBookingId { get; set; }
        public int leg { get; set; }
    }  
    public class VinWonderOptionalViewModel : OptionalViewModel
    {
        public DateTime? DateUsed { get; set; }
        public int Quantity { get; set; }

    }  
    public class TourPackagesOptionalModel : OptionalViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Quantity { get; set; }
        public int Times { get; set; }
    } 
    public class OtherBookingPackagesOptionalModel : OptionalViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int Quantity { get; set; }
    }
    public class OptionalSearshModel
    {
        public int SupplierId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public DateTime? CreateDate
        {
            get
            {
                return DateUtil.StringToDate(CreateDateStr);
            }
        }    
        public DateTime? EndDate
        {
            get
            {
                return DateUtil.StringToDate(EndDateStr);
            }
        }
        public string EndDateStr { get; set; }
        public string CreateDateStr { get; set; }
    }
}
