using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class TourProgramPackage
    {
        public long Id { get; set; }
        public long? TourProductId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool? IsDaily { get; set; }
        public double? AdultPrice { get; set; }
        public double? ChildPrice { get; set; }
        public int? ClientType { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
