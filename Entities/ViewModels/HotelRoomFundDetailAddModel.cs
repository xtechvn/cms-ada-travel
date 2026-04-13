using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.ViewModels
{
    public class HotelRoomFundDetailAddModel
    {
        public int Id { get; set; }
        public int HotelRoomFundId { get; set; }
        public int HotelRoomId { get; set; }
        public decimal NumberOfRooms { get; set; }
        public string StartDateSTR { get; set; }
        public DateTime? StartDate
        {
            get
            {
                if (!string.IsNullOrEmpty(StartDateSTR))
                    return DateUtil.StringToDate(StartDateSTR);
                return null;
            }
        }
        public string EndDateSTR{ get; set; }
        public DateTime? EndDate
        {
            get
            {
                if (!string.IsNullOrEmpty(EndDateSTR))
                    return DateUtil.StringToDate(EndDateSTR);
                return null;
            }
        }
    }
}
