using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class UserReserveHotelRoomFund
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int HotelRoomId { get; set; }
        public int NumberOfRooms { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? Status { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? HotelId { get; set; }
        public int? SupplierId { get; set; }
    }
}
