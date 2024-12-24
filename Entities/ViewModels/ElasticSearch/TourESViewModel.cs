using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
    public class TourESViewModel
    {
        [PropertyName("id")]

        public long id { get; set; }
        [PropertyName("tourname")]

        public string tourname { get; set; }
        [PropertyName("servicecode")]

        public string servicecode { get; set; }
        [PropertyName("organizingtypename")]

        public string organizingtypename { get; set; }
        [PropertyName("tourtypename")]

        public string tourtypename { get; set; }
        [PropertyName("tourtype")]

        public string tourtype { get; set; }
        [PropertyName("startpoint1")]

        public string startpoint1 { get; set; }
        [PropertyName("startpoint2")]

        public string startpoint2 { get; set; }
        [PropertyName("startpoint3")]

        public string startpoint3 { get; set; }
        [PropertyName("supplierid")]

        public string supplierid { get; set; }
        [PropertyName("fullname")]

        public string fullname { get; set; }
        [PropertyName("datedeparture")]

        public string datedeparture { get; set; }
        [PropertyName("groupendpoint")]

        public string groupendpoint { get; set; }
        [PropertyName("createddate")]

        public DateTime createddate { get; set; }
    }
}
