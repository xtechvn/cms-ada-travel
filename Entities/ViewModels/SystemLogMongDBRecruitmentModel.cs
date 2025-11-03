using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Entities.ViewModels
{
    public class SystemLogMongDBRecruitmentModel
    {
        [BsonElement("_id")]
        public string _id { get; set; }
        public void GenID()
        {
            _id = ObjectId.GenerateNewId().ToString();
        }
        public DateTime CreatedTime { get; set; }
        public string name { get; set; }
        public string phone { get; set; }
        public string location { get; set; }
        public string area { get; set; }
        public string email { get; set; }
        public string note { get; set; }
        public string Path { get; set; }
    }
    public class RecruitmentSearchModel
    {
        public string name { get; set; }
        public string phone { get; set; }
        public string location { get; set; }
        public string area { get; set; }
        public string email { get; set; }
        public string FromDateStr { get; set; }
        public DateTime? FromDate
        {
            get
            {
                if (!string.IsNullOrEmpty(FromDateStr))
                {
                    
                    var fromDate = DateUtil.StringToDate(FromDateStr);
                    return new DateTime(fromDate.Value.Year, fromDate.Value.Month, fromDate.Value.Day, 00, 00, 00, DateTimeKind.Local);
                }
                return null;
            }
        }
        public string ToDateStr { get; set; }
        public DateTime? ToDate
        {
            get
            {
                if (!string.IsNullOrEmpty(ToDateStr))
                {
                   
                    var toDate = DateUtil.StringToDate(ToDateStr);
                    return new DateTime(toDate.Value.Year, toDate.Value.Month, toDate.Value.Day, 23, 59, 59, DateTimeKind.Local);
                }
                return null;
            }
        }

    }
}
