using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Document
    {
        public int Id { get; set; }
        public string DocumentName { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Notes { get; set; }
        public byte Status { get; set; }
        public bool IsDeleted { get; set; }
        public string PhysicalStorageLocation { get; set; }
        public string DigitalStorageLocation { get; set; }
        public string ImgLocation { get; set; }
    }
}
