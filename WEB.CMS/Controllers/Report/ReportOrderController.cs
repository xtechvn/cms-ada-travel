using Entities.ViewModels.HotelBooking;
using Entities.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Utilities.Contants;
using WEB.CMS.Customize;
using Repositories.IRepositories;
using Utilities;
using Nest;
using Repositories.Repositories;
using Entities.ViewModels.Report;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace WEB.CMS.Controllers.Report
{
    public class ReportOrderController : Controller
    {

        private readonly IOrderRepository _orderRepository;
        private ManagementUser _ManagementUser;
        private readonly IPaymentRequestRepository _paymentRequestRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IReportRepository _reportRepository;
        public ReportOrderController(IOrderRepository orderRepository, ManagementUser ManagementUser, IPaymentRequestRepository paymentRequestRepository, IWebHostEnvironment webHostEnvironment, IReportRepository reportRepository)
        {
            _ManagementUser = ManagementUser;
            _orderRepository = orderRepository;
            _paymentRequestRepository = paymentRequestRepository;
            _WebHostEnvironment = webHostEnvironment;
            _reportRepository = reportRepository;
        }
        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Search(SearchReportOrderModels searchModel)
        {
            var model = new GenericViewModel<ReportOrderViewModel>();

            try
            {
                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null)
                {

                    model = await _orderRepository.GetlistReportOrder(searchModel);
                    if (model.ListData != null && model.ListData.Count > 0)
                    {
                        foreach (var item in model.ListData)
                        {
                            var CountService = 1;
                            var AllService = await _orderRepository.GetReportOrderAllServiceByOrderId(item.OrderId);
                            if (AllService != null && AllService.Count > 0)
                            {
                                foreach (var service in AllService)
                                {
                                    service.CountPaymentRequest = 2;
                                    switch (service.Type)
                                    {
                                        case "Tour":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.Tourist);
                                            }
                                            break;
                                        case "Khách sạn":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.VINHotelRent);
                                            }
                                            break;
                                        case "Vé máy bay":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.FlyingTicket);
                                            }
                                            break;
                                        case "Dịch vụ khác":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.Other);
                                            }
                                            break;
                                        case "Vinwonder":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.VinWonder);
                                            }
                                            break;
                                    }
                                    if (service.ListPaymentRequest != null && service.ListPaymentRequest.Count > 0)
                                    {
                                        service.CountPaymentRequest = service.ListPaymentRequest.Count + 1;
                                        CountService = service.ListPaymentRequest.Count;
                                    }

                                }
                                if (AllService != null && AllService.Count > 0)
                                {
                                    item.CountService = AllService.Count * 2 + CountService;
                                }
                                item.ListService = AllService;

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - SetServiceController: " + ex);
            }

            return PartialView(model);
        }
        public async Task<IActionResult> ExportExcel(SearchReportOrderModels searchModel)
        {
            try
            {
                var model = new GenericViewModel<ReportOrderViewModel>();
                int _UserId = 0;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    _UserId = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                var current_user = _ManagementUser.GetCurrentUser();
                if (current_user != null)
                {
                    model = await _orderRepository.GetlistReportOrder(searchModel);
                    if (model.ListData != null && model.ListData.Count > 0)
                    {
                        foreach (var item in model.ListData)
                        {
                            var CountService = 1;
                            var AllService = await _orderRepository.GetReportOrderAllServiceByOrderId(item.OrderId);
                            if (AllService != null && AllService.Count > 0)
                            {
                                foreach (var service in AllService)
                                {
                                    service.CountPaymentRequest = 2;
                                    switch (service.Type)
                                    {
                                        case "Tour":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.Tourist);
                                            }
                                            break;
                                        case "Khách sạn":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.VINHotelRent);
                                            }
                                            break;
                                        case "Vé máy bay":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.FlyingTicket);
                                            }
                                            break;
                                        case "Dịch vụ khác":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.Other);
                                            }
                                            break;
                                        case "Vinwonder":
                                            {
                                                service.ListPaymentRequest = _paymentRequestRepository.GetByServiceId(Convert.ToInt32(service.ServiceId), (int)ServicesType.VinWonder);
                                            }
                                            break;
                                    }
                                    if (service.ListPaymentRequest != null && service.ListPaymentRequest.Count > 0)
                                    {
                                        service.CountPaymentRequest = service.ListPaymentRequest.Count + 1;
                                        CountService = service.ListPaymentRequest.Count;
                                    }

                                }
                                if (AllService != null && AllService.Count > 0)
                                {
                                    item.CountService = AllService.Count * 2 + CountService;
                                }
                                item.ListService = AllService;

                            }
                        }
                    }

                }

                string folder = @"\Template\Export\";
                string file_name = StringHelpers.GenFileName("Tổng hợp Yêu cầu ci theo đớn hàng", _UserId, "xlsx");
                string _UploadDirectory = Path.Combine(_WebHostEnvironment.WebRootPath, folder);
                string file_path_combine = Path.Combine(_UploadDirectory, file_name);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                var file_path = await _reportRepository.ExportReportOrder(model.ListData, file_path_combine);

                return Ok(new
                {
                    status = (int)ResponseType.SUCCESS,
                    msg = "Xuất dữ liệu thành công",
                    path = file_path
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Search - OperatorReportController: " + ex);
            }
            return Ok(new
            {
                status = (int)ResponseType.FAILED,
                msg = "Xuất dữ liệu thất bại, vui lòng liên hệ IT",
                path = ""
            });
        }
    }
}
