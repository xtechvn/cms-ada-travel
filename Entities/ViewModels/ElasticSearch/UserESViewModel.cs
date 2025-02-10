using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
    public class UserESViewModel
    {
        public long _id { get; set; } // ID ElasticSearch
        [PropertyName("id")]
        public long id { get; set; } // ID customer
        [PropertyName("username")]
        public string username { get; set; }
        [PropertyName("fullname")]
        public string fullname { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public int? tenantid { get; set; }
    }
}
