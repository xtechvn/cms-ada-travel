using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels
{
    public class UserReserveHotelRoomFundSearchModel
    {
        public int UserId { get; set; }
        public int HotelRoomId { get; set; }
        public int Status { get; set; } = -1;
        public int HotelId { get; set; }
        public int SupplierId { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}
