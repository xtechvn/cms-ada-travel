using Nest;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels
{
    public class HotelESViewModel : HotelModel
    {
        public string _id { get; set; } // ID ElasticSearch
        [PropertyName("id")]

        public long id { get; set; } // ID ElasticSearch
        [PropertyName("telephone")]

        public string telephone { get; set; } // Chuỗi thương hiệu
        [PropertyName("checkintime")]

        public DateTime? checkintime { get; set; }
        [PropertyName("checkouttime")]

        public DateTime? checkouttime { get; set; }

        public void GenID()
        {
            string datetime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + (new Random().Next(100, 999)).ToString();
            _id = datetime;
        }
    }
    public class HotelModel
    {
        [PropertyName("hotelid")]

        public string hotelid { get; set; }
        [PropertyName("name")]

        public string name { get; set; }
        [PropertyName("email")]

        public string email { get; set; }
        [PropertyName("imagethumb")]

        public string imagethumb { get; set; }
        [PropertyName("numberofroooms")]

        public int? numberofroooms { get; set; }
        [PropertyName("star")]


        public double? star { get; set; }
        [PropertyName("reviewcount")]

        public int? reviewcount { get; set; }
        [PropertyName("city")]

        public string city { get; set; }
        [PropertyName("reviewrate")]


        public double? reviewrate { get; set; }
        [PropertyName("country")]

        public string country { get; set; }
        [PropertyName("street")]

        public string street { get; set; }
        [PropertyName("state")]

        public string state { get; set; }
        [PropertyName("hoteltype")]

        public string hoteltype { get; set; }
        [PropertyName("typeofroom")]

        public string typeofroom { get; set; }
        [PropertyName("groupname")]

        public string groupname { get; set; } = null; 
        [PropertyName("index_search")]

        public string index_search { get; set; }
        [PropertyName("isrefundable")]

        public bool? isrefundable { get; set; }
        [PropertyName("isinstantlyconfirmed")]

        public bool? isinstantlyconfirmed { get; set; }
        [PropertyName("isdisplaywebsite")]

        public bool? isdisplaywebsite { get; set; }

    }

}
