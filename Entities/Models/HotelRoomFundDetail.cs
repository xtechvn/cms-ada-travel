using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class HotelRoomFundDetail
    {
        public int Id { get; set; }
        public int HotelRoomFundId { get; set; }
        public int HotelRoomId { get; set; }
        public decimal Amount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual HotelRoomFund HotelRoomFund { get; set; }
    }
}
