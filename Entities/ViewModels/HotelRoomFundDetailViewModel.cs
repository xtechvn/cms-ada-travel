using Entities.Models;
using System;
using System.Collections.Generic;

namespace Entities.ViewModels
{
    /// <summary>
    /// ViewModel cho trang chi tiết quỹ phòng khách sạn (hiển thị dạng lịch theo ngày)
    /// </summary>
    public class HotelRoomFundDetailViewModel
    {
        public int Id { get; set; }
        public string SupplierName { get; set; }
        public string HotelName { get; set; }
        public decimal? TotalRooms { get; set; }

        /// <summary>
        /// Danh sách ngày hiển thị trên header (7 ngày)
        /// </summary>
        public List<DateTime> DisplayDates { get; set; } = new List<DateTime>();

        /// <summary>
        /// Danh sách hạng phòng với dữ liệu theo ngày
        /// </summary>
        public List<RoomCategoryRow> RoomCategories { get; set; } = new List<RoomCategoryRow>();
    }

    /// <summary>
    /// Mỗi hạng phòng là 1 dòng trong bảng
    /// </summary>
    public class RoomCategoryRow
    {
        public int HotelRoomId { get; set; }
        public string RoomName { get; set; }

        /// <summary>
        /// Tổng số phòng (capacity) của hạng phòng này
        /// </summary>
        public decimal TotalCapacity { get; set; }

        /// <summary>
        /// Dữ liệu quỹ phòng theo từng ngày (key = ngày, value = số phòng đã phân bổ)
        /// </summary>
        public Dictionary<DateTime, decimal> DailyAllocations { get; set; } = new Dictionary<DateTime, decimal>();

        /// <summary>
        /// Dữ liệu phòng đã đặt theo từng ngày (key = ngày, value = số phòng đã đặt)
        /// </summary>
        public Dictionary<DateTime, decimal> DailyBooked { get; set; } = new Dictionary<DateTime, decimal>();
    }
}
