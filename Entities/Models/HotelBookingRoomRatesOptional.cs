using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class HotelBookingRoomRatesOptional
    {
        public long Id { get; set; }
        public long HotelBookingRoomOptionalId { get; set; }
        public long HotelBookingRoomRatesId { get; set; }
        public double Price { get; set; }
        public double Profit { get; set; }
        public double TotalAmount { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        /// <summary>
        /// Giá điều hành nhập 1 phòng/1 đêm
        /// </summary>
        public double? OperatorPrice { get; set; }
    }
}
