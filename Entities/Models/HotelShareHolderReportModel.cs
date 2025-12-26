using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class HotelShareHolderReportModel
    {
        public int ShareHolderId { get; set; }
        public string ShareHolderName { get; set; }

        public decimal SharePercent { get; set; }

        public decimal TotalProfitByShare { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainAmount { get; set; }
    }
    public class ReportHotelShareHolderViewModel
    {
        public int ShareHolderId { get; set; }
        public string FullName { get; set; }
        public decimal SharePercent { get; set; }

        public decimal TotalProfitByPercent { get; set; }
        public decimal TotalPaid { get; set; }
        public decimal RemainToPay { get; set; }

        public int TotalRow { get; set; }
    }
    public class ReportHotelShareHolderSearchModel
    {
        // 🔍 Tên cổ đông (search theo FullName / UserName / Phone tuỳ SP)
        public string ShareHolderName { get; set; }

        // 📄 Phân trang
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        // ✅ Constructor mặc định cho khỏi null
        
    }
    public class ReportHotelShareHolderDetailViewModel
    {
        public int HotelId { get; set; }
        public string HotelName { get; set; }

        public decimal HotelProfit { get; set; }
        public decimal SharePercent { get; set; }
        public decimal ProfitByShare { get; set; }
    }



}
