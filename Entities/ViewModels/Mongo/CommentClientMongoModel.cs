using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Mongo
{
    public class CommentClientMongoModel
    {
        public string _id { get; set; }
        public string Note { get; set; }
        public string ClientId { get; set; }
        public string FullName { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}
