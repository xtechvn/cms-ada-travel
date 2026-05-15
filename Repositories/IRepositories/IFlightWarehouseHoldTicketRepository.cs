using Entities.ViewModels.FlightWarehouse;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entities.Models;

namespace Repositories.IRepositories
{
    public interface IFlightWarehouseHoldTicketRepository
    {
        Task<GenericViewModel<FlightWarehouseHoldTicketViewModel>> GetList(FlightWarehouseHoldTicketSearchModel searchModel);
        Task<Entities.Models.FlightWarehouseHoldTicket> GetById(long id);
        Task<int> UpdateFlightWarehouseHoldTicketStatus(long id, int Status);
        Task<int> UpdateFlightWarehouseHoldTicket(FlightWarehouseHoldTicket model);
    }
}
