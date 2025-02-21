using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class HotelBookingRoomRate
    {
        public long Id { get; set; }
        public long HotelBookingRoomId { get; set; }
        public string RatePlanId { get; set; }
        public DateTime StayDate { get; set; }
        public double Price { get; set; }
        public double Profit { get; set; }
        public double TotalAmount { get; set; }
        public string AllotmentId { get; set; }
        public string RatePlanCode { get; set; }
        public string PackagesInclude { get; set; }
        public double? UnitPrice { get; set; }
        public short? Nights { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// Giá điều hành nhập 1 phòng/1 đêm
        /// </summary>
        public double? OperatorPrice { get; set; }
        /// <summary>
        /// Giá sale nhập 1 phòng/1 đêm
        /// </summary>
        public double? SalePrice { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
