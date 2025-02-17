using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
   public class ContractNoESViewModel
    {
        [PropertyName("id")]
        public long id { get; set; }
        [PropertyName("contractno")]
        public string contractno { get; set; }
        public int? tenantid { get; set; }
    }
}
