using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;

namespace WEB.CMS.Controllers
{
    public class ratingsController : Controller
    {
        private readonly IUserRepository _UserRepository;
        public ratingsController(IUserRepository userRepository)
        {
            _UserRepository = userRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult SearchTop(UserProfitReportModel model)
        {
            model.PageSize = 3;
            model.PageIndex = 1;
            var data = _UserRepository.GetListUserProfitReport(model);
            return PartialView(data);
        }
        public IActionResult Search(UserProfitReportModel model)
        {
            ViewBag.pageIndex = model.PageIndex;
            ViewBag.PageSize = model.PageSize;
            model.Type = 2;
            var data2 = _UserRepository.GetListUserProfitReport(model);
            return PartialView(data2);
        }
    }
}
