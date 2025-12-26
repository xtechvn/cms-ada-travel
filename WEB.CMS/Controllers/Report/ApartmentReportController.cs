using Entities.ViewModels.Report;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using Utilities;
using Repositories.Repositories;
using Entities.Models;

namespace WEB.CMS.Controllers.Report
{
    public class ApartmentReportController : Controller
    {
        private readonly IApartmentOrderRepository _apartmentOrderRepository;
        
        private readonly IWebHostEnvironment _WebHostEnvironment;

        public ApartmentReportController(IApartmentOrderRepository apartmentOrderRepository, IWebHostEnvironment hostEnvironment)
        {
            _apartmentOrderRepository = apartmentOrderRepository;
            _WebHostEnvironment = hostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

 
        [HttpPost]
        public IActionResult Search(ReportHotelShareHolderSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<ReportHotelShareHolderViewModel>();
            try
            {
                searchModel.PageIndex = currentPage;
                searchModel.PageSize = pageSize;

                var data = _apartmentOrderRepository.GetReportHotelShareHolder(searchModel, out long total);

                model.ListData = data;
                model.CurrentPage = currentPage;
                model.PageSize = pageSize;
                model.TotalRecord = total;
                model.TotalPage = (int)Math.Ceiling((double)total / pageSize);

                return PartialView(model);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - ReportHotelShareHolder: " + ex);
                return PartialView();
            }
        }


        public IActionResult Detail(int shareHolderId, string fullName)
        {
            ViewBag.ShareHolderName = fullName;
            var data = _apartmentOrderRepository
        .GetShareHolderDetail(shareHolderId);

            return PartialView("_ReportHotelShareHolderDetail", data);

        }

    }
}
