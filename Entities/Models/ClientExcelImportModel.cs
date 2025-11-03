using Entities.ViewModels.CustomerManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class ClientExcelImportModel: CustomerManagerView
    {
        public int DepartmentId { get; set; }
    }
}
