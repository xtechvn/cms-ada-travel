using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public partial class HotelShareHolder
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int UserId { get; set; }

        public decimal SharePercent { get; set; }
        public string Note { get; set; }

        public int? CreatedBy { get; set; }
        public DateTime? CreateDate { get; set; }

        public int? UpdatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }

        public bool IsDeleted { get; set; }

        // ✅ CHỈ DÙNG ĐỂ HIỂN THỊ - KHÔNG MAP DB
        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string FullName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string UserName { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Phone { get; set; }

        [System.ComponentModel.DataAnnotations.Schema.NotMapped]
        public string Email { get; set; }
    }


    public class HotelShareHolderGridModel
    {
        public int Id { get; set; }
        public int HotelId { get; set; }
        public int UserId { get; set; }

        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public decimal SharePercent { get; set; }
        public string Note { get; set; }
        public string UserName { get; set; }

        public string UserCreate { get; set; }
        public DateTime CreateDate { get; set; }

        public int TotalRow { get; set; }
    }


}
