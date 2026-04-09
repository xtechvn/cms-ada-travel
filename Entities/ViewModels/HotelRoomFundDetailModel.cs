using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class HotelRoomFundDetailModel: HotelRoomFundDetail
    {
        public string RoomName { get; set; }
        public int TotalRoomNights { get; set; }
        public int TotalBookedRooms { get; set; }
        public int NumberOfRoomsAvailable { get; set; }
    }
}
