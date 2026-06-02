using DAL.Tourdepartureschedule;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.Tourdepartureschedule;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using Repositories.Repositories.BaseRepos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class TourDepartureRepository : BaseRepository, ITourDepartureRepository
    {
        private readonly TourDepartureDAL _tourDepartureDAL;
        private readonly TourPriceDAL _tourPriceDAL;
        private readonly TourItineraryDAL _tourItineraryDAL;

        public TourDepartureRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig, IUserRepository userRepository, IConfiguration configuration) : base(context, dataBaseConfig, configuration, userRepository)
        {
            _tourDepartureDAL = new TourDepartureDAL(_SqlServerConnectString);
            _tourPriceDAL = new TourPriceDAL(_SqlServerConnectString);
            _tourItineraryDAL = new TourItineraryDAL(_SqlServerConnectString);
        }

        public async Task<int> InsertTourDeparture(TourDeparture Model)
        {
            return await _tourDepartureDAL.InsertTourDeparture(Model);
        }

        public async Task<int> UpdateTourDeparture(TourDeparture Model)
        {
            return await _tourDepartureDAL.UpdateTourDeparture(Model);
        }

        public async Task<DetailTourDepartureModel> GetTourDepartureDetailById(int Id)
        {
            var model = await _tourDepartureDAL.GetTourDepartureDetailById(Id);
            if (model != null)
            {
                model.TourItineraries = await _tourItineraryDAL.GetTourItineraryByDepartureId(Id);
                model.TourPrices = await _tourPriceDAL.GetTourPriceByDepartureId(Id);
            }
            return model;
        }

        public async Task<List<GetListTourDepartureModel>> GetListTourDeparture(TourDepartureSeachModel model)
        {
            return await _tourDepartureDAL.GetListTourDeparture(model);
        }

        public async Task<int> SaveTourDeparture(TourDepartureSaveModel model)
        {
            int result = 0;
            if (model.Id > 0)
            {
                result = await _tourDepartureDAL.UpdateTourDeparture(model);
            }
            else
            {
                result = await _tourDepartureDAL.InsertTourDeparture(model);
                if (result > 0) model.Id = result;
            }

            if (result > 0)
            {
                if (model.TourItineraries != null)
                {
                    foreach (var itinerary in model.TourItineraries)
                    {
                        itinerary.TourDepartureId = model.Id;
                        if (itinerary.Id > 0)
                        {
                            await _tourItineraryDAL.UpdateTourItinerary(itinerary);
                        }
                        else
                        {
                            await _tourItineraryDAL.InsertTourItinerary(itinerary);
                        }
                    }
                }

                if (model.TourPrices != null)
                {
                    foreach (var price in model.TourPrices)
                    {
                        price.TourDepartureId = model.Id;
                        if (price.Id > 0)
                        {
                            await _tourPriceDAL.UpdateTourPrice(price);
                        }
                        else
                        {
                            await _tourPriceDAL.InsertTourPrice(price);
                        }
                    }
                }
            }

            return result;
        }
    }
}
