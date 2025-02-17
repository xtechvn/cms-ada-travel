using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
  public class HotelBookingCodeESViewModel
    {
        [PropertyName("id")]

        public int id { get; set; }
        [PropertyName("orderid")]

        public int orderid { get; set; }
        [PropertyName("serviceid")]

        public int serviceid { get; set; }
        [PropertyName("bookingcode")]


        public string bookingcode { get; set; }
        [PropertyName("listorderid")]

        public string listorderid { get; set; }
        [PropertyName("isdelete")]

        public bool isdelete { get; set; }
        [PropertyName("tenantid")]

        public int? tenantid { get; set; }


    }
}
