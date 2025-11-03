using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Mongo
{
   public class PaymentRequestMongoModel
    {
        public string _id { get; set; }
        public long PaymentRequestId { get; set; }
        public long Count { get; set; }
        public string CreatedUserName { get; set; }   
        public DateTime CreatedTime { get; set; }
    }
}
