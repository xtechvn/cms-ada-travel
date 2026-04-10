using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class UserReserveHotelRoomFundViewModel: UserReserveHotelRoomFund
    {
        public string HotelName { get; set; }
        public string RoomName { get; set; }
        public string SupplierName { get; set; }
        public string UserName { get; set; }
        public int TotalRow { get; set; }
    }
}
