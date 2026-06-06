using APP_CHECKOUT.RabitMQ;
using DAL.Tourdepartureschedule;
using Entities.Models;
using Entities.ViewModels.Tourdepartureschedule;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Threading.Tasks;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.Adavigo.CMS.Controllers
{
    [CustomAuthorize]
    public class TourDepartureController : Controller
    {
        private readonly ITourDepartureRepository _tourDepartureRepository;
        private readonly IAllCodeRepository _allCodeRepository;
        private readonly ITourRepository _tourRepository;

        public TourDepartureController(ITourDepartureRepository tourDepartureRepository, IAllCodeRepository allCodeRepository, ITourRepository tourRepository)
        {
            _tourDepartureRepository = tourDepartureRepository;
            _allCodeRepository = allCodeRepository;
            _tourRepository = tourRepository;

        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(TourDepartureSeachModel searchModel)
        {
            try
            {
                var data = await _tourDepartureRepository.GetListTourDeparture(searchModel);
                ViewBag.searchModel = searchModel;
                return PartialView("_Search", data);
            }
            catch (Exception ex)
            {
                return PartialView("_Search", null);
            }
        }

        public async Task<IActionResult> AddOrUpdate(long id)
        {
            var model = new DetailTourDepartureModel();
            ViewBag.allCode = _allCodeRepository.GetListByType(AllCodeType.TOUR_DEPARTURE_STATUS);
            ViewBag.itineraryGo = new TourItinerary();
            ViewBag.itineraryBack = new TourItinerary();
            ViewBag.priceRetail = null;
            ViewBag.priceAgent = null;
            ViewBag.TourProduct = null;
            if (id > 0)
            {
                model = await _tourDepartureRepository.GetTourDepartureDetailById((int)id);
                var product = await _tourRepository.GetTourProductById(model.TourProductId == null ? 0 : (long)model.TourProductId);
                if (product != null && product.Id > 0)
                {
                    ViewBag.TourProduct = product;
                }
                ViewBag.itineraryGo = model?.TourItineraries?.FirstOrDefault(x => x.RouteType == 1) ?? new TourItinerary() { TransportType = 3 };
                ViewBag.itineraryBack = model?.TourItineraries?.FirstOrDefault(x => x.RouteType == 2) ?? new TourItinerary() { TransportType = 3 };
                ViewBag.priceRetail = model!=null && model.TourPrices !=null && model.TourPrices.Count() >0 ? model.TourPrices.FirstOrDefault(x => x.PriceType == 1) :null;
                ViewBag.priceAgent = model != null && model.TourPrices != null && model.TourPrices.Count() > 0 ? model.TourPrices.FirstOrDefault(x => x.PriceType == 2) :null ;
            }
           
        
            return View(model);
        }

        public async Task<IActionResult> Detail(long id)
        {
            DetailTourDepartureModel model = new DetailTourDepartureModel();
            ViewBag.allCode = _allCodeRepository.GetListByType(AllCodeType.TOUR_DEPARTURE_STATUS);
            
            if (id > 0)
            {
                model = await _tourDepartureRepository.GetTourDepartureDetailById((int)id);
            }
            if (model == null) return NotFound();

            ViewBag.itineraryGo = model?.TourItineraries?.FirstOrDefault(x => x.RouteType == 1) ?? new TourItinerary { TransportType = 3 };
            ViewBag.itineraryBack = model?.TourItineraries?.FirstOrDefault(x => x.RouteType == 2) ?? new TourItinerary { TransportType = 3 };
            ViewBag.priceRetail = model?.TourPrices?.FirstOrDefault(x => x.PriceType == 1);
            ViewBag.priceAgent = model?.TourPrices?.FirstOrDefault(x => x.PriceType == 2);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] TourDepartureSaveModel model)
        {
            try
            {
                long result = await _tourDepartureRepository.SaveTourDeparture(model);

                if (result > 0)
                {

                    return Ok(new { status = 1, msg = "Thành công" });
                }
                else
                {
                    return Ok(new { status = 0, msg = "Thất bại" });
                }
            }
            catch (Exception ex)
            {
                return Ok(new { status = 0, msg = "Lỗi hệ thống: " + ex.Message });
            }
        }
    }
}
