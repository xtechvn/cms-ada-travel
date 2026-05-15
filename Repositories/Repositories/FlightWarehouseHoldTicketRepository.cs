using DAL.FlightWarehouse;
using Entities.ConfigModels;
using Entities.ViewModels.FlightWarehouse;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Entities.Models;

namespace Repositories.Repositories
{
    public class FlightWarehouseHoldTicketRepository: IFlightWarehouseHoldTicketRepository
    {
        private readonly FlightWarehouseHoldTicketDAL flightWarehouseHoldTicketDAL;
        public FlightWarehouseHoldTicketRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            var connectionString = dataBaseConfig.Value.SqlServer.ConnectionString;

            flightWarehouseHoldTicketDAL = new FlightWarehouseHoldTicketDAL(connectionString);
        }
        public async Task<GenericViewModel<FlightWarehouseHoldTicketViewModel>> GetList(FlightWarehouseHoldTicketSearchModel searchModel)
        {
            try
            {
                var data = await flightWarehouseHoldTicketDAL.GetList(searchModel);
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
                LogHelper.InsertLogTelegram("GetList - FlightWarehouseRepository: " + ex);
            }

            return null;
        }

        public async Task<FlightWarehouseHoldTicket> GetById(long id)
        {
            try
            {
                return await flightWarehouseHoldTicketDAL.GetById(id);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseHoldTicketRepository: " + ex);
                return null;
            }
        }   
        public async Task<int> UpdateFlightWarehouseHoldTicket(FlightWarehouseHoldTicket model)
        {
            try
            {
                return await flightWarehouseHoldTicketDAL.UpdateFlightWarehouseHoldTicket(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseHoldTicketRepository: " + ex);
                return -1;
            }
        } public async Task<int> UpdateFlightWarehouseHoldTicketStatus(long id,int Status)
        {
            try
            {
                return await flightWarehouseHoldTicketDAL.UpdateFlightWarehouseHoldTicketStatus(id, Status);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetById - FlightWarehouseHoldTicketRepository: " + ex);
                return -1;
            }
        }
    }
}
