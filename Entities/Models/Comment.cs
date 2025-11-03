using System;
using System.Collections.Generic;

namespace Entities.Models
{
    public partial class Comment
    {
        public int Id { get; set; }
        public int RequestId { get; set; }
        public string Content { get; set; }
        public string AttachFile { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AttachFileName { get; set; }
        public int? UserType { get; set; }
    }
}
