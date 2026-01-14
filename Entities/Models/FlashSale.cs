using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class FlashSale
    {
        public int Id { get; set; }
        public byte Status { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateLast { get; set; }
        public long UserUpdateId { get; set; }
        public int UserCreateId { get; set; }
       // public string? UserCreateName { get; set; } // JOIN thêm User
        public string Name { get; set; }
    }


}
