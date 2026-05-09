using Entities.Models;
using Entities.ViewModels.FlightWarehouse;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IFlightWarehouseRepository
    {
        Task<DataTable> GetListFlightWarehouse(GetListFlightWarehouseModel searchModel, int pageIndex, int pageSize);
        Task<long> UpsertBooking(FlightWarehouseBooking model);
        Task<long> UpsertSegment(FlightWarehouseSegment model);
        Task<long> UpsertPrice(FlightWarehousePrice model);
        Task<FlightWarehouseBookingModel> GetBookingById(long id);
        Task<List<FlightWarehouseSegmentModel>> GetSegmentsByBookingId(long bookingId);
        Task<List<FlightWarehousePriceModel>> GetPricesByBookingId(long bookingId);
    }
}
