using Entities.Models;
using Entities.ViewModels.Tourdepartureschedule;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ITourDepartureRepository
    {
        Task<int> InsertTourDeparture(TourDeparture Model);
        Task<int> UpdateTourDeparture(TourDeparture Model);
        Task<DetailTourDepartureModel> GetTourDepartureDetailById(int Id);
        Task<List<GetListTourDepartureModel>> GetListTourDeparture(TourDepartureSeachModel model);
        Task<int> SaveTourDeparture(TourDepartureSaveModel model);
    }
}
