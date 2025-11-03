using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    // ViewModels/PagedResult.cs
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalRecords / PageSize);
    }

    // ViewModels/TicketListItemViewModel.cs
    public class TicketListItemViewModel
    {
        public int TicketId { get; set; }
        public string TicketCode { get; set; }
        public int? SupplierId { get; set; }     // 👈 thêm dòng này để đồng bộ với Ticket
        public string SupplierName { get; set; }
        public int? Category { get; set; }     // hiện INT theo DB mới
        public int? TicketType { get; set; }   // hiện INT theo DB mới
        public int? PlayZone { get; set; }     // hiện INT theo DB mới
        public int? Status { get; set; }       // hiện INT theo DB mới
        public DateTime? ImportDate { get; set; }
        public DateTime? ExpiredDate { get; set; }
        public DateTime? SoldDuration { get; set; }

        public string SoldDurationDisplay
        {
            get
            {
                if (!SoldDuration.HasValue) return "";
                var diff = DateTime.Now - SoldDuration.Value;

                if (diff.TotalDays < 7)
                    return SoldDuration.Value.ToString("dd/MM/yyyy HH:mm:ss");

                int months = (int)(diff.TotalDays / 30);
                int days = (int)(diff.TotalDays % 30);
                return $"{months} tháng {days} ngày";
            }
        }
        public string? QRCode { get; set; }       // 👈 thêm cho đồng bộ SP_Insert
        public string CategoryName { get; set; }

        public string TicketTypeName { get; set; }
        public string PlayZoneName { get; set; }
        public string StatusName { get; set; }

        public DateTime? CreatedDate { get; set; }
        // Tổng dòng (nếu dùng cho paging)
        public int TotalRow { get; set; }
    }
    public class SupplierTicketViewModel
    {
        public int SupplierId { get; set; }
        public string FullName { get; set; }
        public int QuantityTickets { get; set; }
        public int TotalRow { get; set; }
    }

    public class Ticket
    {
        public int TicketId { get; set; }

        public string TicketCode { get; set; }

        public int? SupplierId { get; set; }

        public string SupplierName { get; set; }

        public int? Category { get; set; }      // INT (FK tới bảng Category)
        public int? TicketType { get; set; }    // INT (FK tới bảng TicketType)
        public int? PlayZone { get; set; }      // INT (FK tới bảng PlayZone)
        public int? Status { get; set; }        // INT (FK tới bảng TicketStatus)

        public DateTime? ImportDate { get; set; }

        public DateTime? ExpiredDate { get; set; }

        public string SoldDuration { get; set; }

        public string QRCode { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        
    }

    // ViewModels/TicketListFilter.cs
    public class TicketListFilter
    {
        public int SupplierId { get; set; }

        // 🔍 Tìm kiếm (mã vé, tên khu, danh mục, v.v.)
        public string Search { get; set; }

        // ⚙️ Trạng thái (CodeValue từ AllCode)
        public int? Status { get; set; }

        // 🗺️ Khu vực vui chơi (PlayZoneId)
        public int? PlayZoneId { get; set; }

        // 📁 Danh mục (CategoryId)
        public int? CategoryId { get; set; }

        // 🎟️ Loại vé (TicketTypeId)
        public int? TicketTypeId { get; set; }

        // 📅 Ngày hết hạn (lọc đúng ngày hoặc nhỏ hơn ngày chọn)
        public DateTime? ExpiredDate { get; set; }

        // 🔢 Phân trang
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

}
