using DAL.FlightWarehouse;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
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
        private readonly FlightWarehouseHoldTicketDAL flightWarehouseHoldTicketDAL;

        public FlightWarehouseRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var connectionString = dataBaseConfig.Value.SqlServer.ConnectionString;
            flightWarehouseBookingDAL = new FlightWarehouseBookingDAL(connectionString);
            flightWarehouseSegmentDAL = new FlightWarehouseSegmentDAL(connectionString);
            flightWarehousePriceDAL = new FlightWarehousePriceDAL(connectionString);
            flightWarehouseHoldTicketDAL = new FlightWarehouseHoldTicketDAL(connectionString);
        }

        public async Task<GenericViewModel<FlightWarehouseBookingViewModel>> GetListFlightWarehouse(GetListFlightWarehouseModel searchModel, int pageIndex, int pageSize)
        {
            try
            {
                var data = await flightWarehouseBookingDAL.GetListFlightWarehouse(searchModel, pageIndex, pageSize);
                var model = new GenericViewModel<FlightWarehouseBookingViewModel>();
                model.ListData = data;
                model.TotalRecord = data != null && data.Count > 0 ? data[0].TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / pageSize);
                model.CurrentPage = pageIndex;
                model.PageSize = pageSize;
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListFlightWarehouse - FlightWarehouseRepository: " + ex);
            }

            return null;
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

        public async Task<GenericViewModel<FlightWarehouseHoldTicketViewModel>> GetListHoldTicket(FlightWarehouseHoldTicketSearchModel searchModel)
        {
            try
            {
                var data = await flightWarehouseHoldTicketDAL.GetListFlightWarehouseHoldTicket(searchModel);
                var model = new GenericViewModel<FlightWarehouseHoldTicketViewModel>();
                model.ListData = data;
                model.TotalRecord = data != null && data.Count > 0 ? data[0].TotalRow : 0;
                model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / searchModel.pageSize);
                model.CurrentPage = searchModel.pageIndex;
                model.PageSize = searchModel.pageSize;
                return model;
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListHoldTicket - FlightWarehouseRepository: " + ex);
            }

            return null;
        }

        public async Task<long> UpsertHoldTicket(FlightWarehouseHoldTicket model)
        {
            return await flightWarehouseHoldTicketDAL.InsertFlightWarehouseHoldTicket(model);
        }
    }
}
