using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Repositories.IRepositories
{
    public interface ITicketRepository
    {
        TicketListItemViewModel GetDetail(int id);
        TicketListItemViewModel GetByCode(string code);
        TicketListItemViewModel InsertTicket(TicketListItemViewModel model);
        bool UpdateTicket(TicketListItemViewModel model);
        bool UpdateStatusByCode(string code, int status);
        PagedResult<TicketListItemViewModel> GetListBySupplier(TicketListFilter filter);
       
    }
}
