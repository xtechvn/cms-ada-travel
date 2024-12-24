using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
   public class ProgramsViewModel
    {
        [PropertyName("id")]
        public long id { get; set; }
        [PropertyName("servicename")]
        public string servicename { get; set; }
        [PropertyName("programname")]
        public string programname { get; set; }
        [PropertyName("programcode")]
        public string programcode { get; set; }
        public string hotelid { get; set; }
        public long status  { get; set; }

    }
}
