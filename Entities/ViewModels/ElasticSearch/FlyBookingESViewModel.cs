using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
    public class FlyBookingESViewModel
    {
        [PropertyName("id")]

        public long id { get; set; }
        [PropertyName("orderid")]

        public long orderid { get; set; }
        [PropertyName("status")]

        public string status { get; set; }
        [PropertyName("bookingcode")]

        public string bookingcode { get; set; }
        [PropertyName("flight")]

        public string flight { get; set; }
        [PropertyName("amount")]

        public double amount { get; set; }
        [PropertyName("expirydate")]

        public DateTime expirydate { get; set; }
        [PropertyName("leg")]

        public int leg { get; set; }
        [PropertyName("startdate")]

        public DateTime startdate { get; set; }
        [PropertyName("enddate")]

        public DateTime enddate { get; set; }
        [PropertyName("servicecode")]

        public string servicecode { get; set; }
        [PropertyName("groupbookingid")]

        public string groupbookingid { get; set; }
        [PropertyName("startpoint")]

        public string startpoint { get; set; }
        [PropertyName("endpoint")]

        public string endpoint { get; set; }
    }
}
