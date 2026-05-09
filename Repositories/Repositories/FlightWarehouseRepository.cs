using DAL.FlightWarehouse;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.FlightWarehouse;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Utilities;

namespace Repositories.Repositories
{
    public class FlightWarehouseRepository : IFlightWarehouseRepository
    {
        private readonly FlightWarehouseBookingDAL flightWarehouseBookingDAL;
        private readonly FlightWarehouseSegmentDAL flightWarehouseSegmentDAL;
        private readonly FlightWarehousePriceDAL flightWarehousePriceDAL;

        public FlightWarehouseRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var connectionString = dataBaseConfig.Value.SqlServer.ConnectionString;
            flightWarehouseBookingDAL = new FlightWarehouseBookingDAL(connectionString);
            flightWarehouseSegmentDAL = new FlightWarehouseSegmentDAL(connectionString);
            flightWarehousePriceDAL = new FlightWarehousePriceDAL(connectionString);
        }

        public async Task<DataTable> GetListFlightWarehouse(GetListFlightWarehouseModel searchModel, int pageIndex, int pageSize)
        {
            return await flightWarehouseBookingDAL.GetListFlightWarehouse(searchModel, pageIndex, pageSize);
        }

        public async Task<long> UpsertBooking(FlightWarehouseBooking model)
        {
            return await flightWarehouseBookingDAL.Upsert(model);
        }

        public async Task<long> UpsertSegment(FlightWarehouseSegment model)
        {
            return await flightWarehouseSegmentDAL.Upsert(model);
        }

        public async Task<long> UpsertPrice(FlightWarehousePrice model)
        {
            return await flightWarehousePriceDAL.Upsert(model);
        }

        public async Task<FlightWarehouseBookingModel> GetBookingById(long id)
        {
            return await flightWarehouseBookingDAL.GetById(id);
        }

        public async Task<List<FlightWarehouseSegmentModel>> GetSegmentsByBookingId(long bookingId)
        {
            return await flightWarehouseSegmentDAL.GetByBookingId(bookingId);
        }

        public async Task<List<FlightWarehousePriceModel>> GetPricesByBookingId(long bookingId)
        {
            return await flightWarehousePriceDAL.GetByBookingId(bookingId);
        }
    }
}
