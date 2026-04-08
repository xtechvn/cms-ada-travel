using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class HotelRoomFund
    {
        public HotelRoomFund()
        {
            HotelRoomFundDetails = new HashSet<HotelRoomFundDetail>();
        }

        public int Id { get; set; }
        public int HotelId { get; set; }
        public int? SupplierId { get; set; }
        public decimal? TotalAmount { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string CreateBy { get; set; }
        public string UpdateBy { get; set; }

        public virtual ICollection<HotelRoomFundDetail> HotelRoomFundDetails { get; set; }
    }
}
