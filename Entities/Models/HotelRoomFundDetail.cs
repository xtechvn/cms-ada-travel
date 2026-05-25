using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace Entities.Models
{
    public partial class HotelRoomFundDetail
    {
        public int Id { get; set; }
        public int HotelRoomFundId { get; set; }
        public int HotelRoomId { get; set; }
        public decimal NumberOfRooms { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public int HardFundRoom { get; set; }//Số lượng phòng quỹ cứng
        public int SoftFundRoom { get; set; }//Số lượng phòng quỹ mềm
        public DateTime? ExpiredDate { get; set; }//Ngày hết hạn
        public int Number { get; set; }//số Ngày hết hạn trước
        public virtual HotelRoomFund HotelRoomFund { get; set; }
    }
}
