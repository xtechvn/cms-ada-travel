﻿using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.DashBoard;
using Entities.ViewModels.TransferSms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.Adavigo.CMS.Service;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class DashBoardController : Controller
    {
        private readonly IDashboardRepository _DashboardRepository;
        private readonly IOrderRepository _OrderRepository;
        private readonly IPaymentVoucherRepository _PaymentVoucherRepository;
        private ManagementUser _ManagementUser;
        private IAllCodeRepository _allCodeRepository;

        public DashBoardController(ManagementUser managementUser,
            IDashboardRepository dashboardRepository,
            IOrderRepository orderRepository, IPaymentVoucherRepository PaymentVoucherRepository, IAllCodeRepository allCodeRepository)
        {
            _DashboardRepository = dashboardRepository;
            _ManagementUser = managementUser;
            _OrderRepository = orderRepository;
            _PaymentVoucherRepository = PaymentVoucherRepository;
            _allCodeRepository = allCodeRepository;
        }


        [HttpPost]
        public IActionResult GetNewClientByDay(DashboardSearchModel model)
        {
            try
            {
                var data = _DashboardRepository.GetNewClientByDay(model.from_date.Date, model.to_date);
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetNewClientByDay - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }

        [HttpPost]
        public IActionResult GetRevenueOrderByDay(DashboardSearchModel model)
        {
            try
            {
                var data = _DashboardRepository.GetRevenueOrderByDay(model.from_date, model.to_date, model.status);
                if (data != null && data.Rows.Count > 0)
                {
                    return new JsonResult(data.Rows[0]);
                }
                else
                {
                    return new JsonResult(new
                    {
                        TotalOrder = 0,
                        TotalRevenue = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenueOrderByDay - DashBoardController: " + ex.ToString());
                return Content("");
            }

        }

        [HttpPost]
        public IActionResult GetRevenueOrderGroupBySale(DashboardSearchModel model)
        {
            try
            {
                var dataTable = _DashboardRepository.GetRevenueOrderGroupBySale(model.from_date, model.to_date, model.type);

                switch (model.type)
                {
                    case 1:
                        var data = dataTable.AsEnumerable().Select(s => new
                        {
                            date = DateTime.Parse(s["Date"].ToString()),
                            revenue = decimal.Parse(s["TotalRevenue"].ToString())
                        });

                        if (model.date_type == 1)
                        {
                            return new JsonResult(new
                            {
                                label = data.Select(s => s.date.ToString("dd/MM/yyyy")),
                                value = data.Select(s => s.revenue)
                            });
                        }
                        else
                        {
                            var months = data.GroupBy(s => new { s.date.Month, s.date.Year }).Select(s => new
                            {
                                label = s.First().date.ToString("MM/yyyy"),
                                revenue = s.Sum(m => m.revenue)
                            });

                            return new JsonResult(new
                            {
                                label = months.Select(s => s.label),
                                value = months.Select(s => s.revenue)
                            });
                        }
                    case 2:
                    case 3:
                        var datas = dataTable.AsEnumerable().Select(s => new
                        {
                            label = s["DimName"].ToString(),
                            revenue = decimal.Parse(s["TotalRevenue"].ToString())
                        });

                        return new JsonResult(new
                        {
                            label = datas.Select(s => s.label),
                            value = datas.Select(s => s.revenue)
                        });
                }

                return new JsonResult(null);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetRevenueOrderGroupBySale - DashBoardController: " + ex.ToString());
                return Content("");
            }

        }

        [HttpPost]
        public async Task<IActionResult> GetOrderStatistic()
        {
            try
            {
                var current_user = _ManagementUser.GetCurrentUser();

                var searchModel = new OrderViewSearchModel();
                if (current_user != null && !string.IsNullOrEmpty(current_user.Role))
                {
                    var list = current_user.Role.Split(',');

                    foreach (var item in list)
                    {
                        switch (Convert.ToInt32(item))
                        {
                            case (int)RoleType.SaleOnl:
                            case (int)RoleType.SaleTour:
                            case (int)RoleType.SaleKd:
                            case (int)RoleType.TPDHKS:
                            case (int)RoleType.TPDHVe:
                            case (int)RoleType.TPDHTour:
                            case (int)RoleType.TPKS:
                            case (int)RoleType.TPTour:
                            case (int)RoleType.TPVe:
                            case (int)RoleType.DHPQ:
                            case (int)RoleType.DHTour:
                            case (int)RoleType.DHVe:
                            case (int)RoleType.DHKS:
                            case (int)RoleType.GDHN:
                            case (int)RoleType.GDHPQ:
                                {
                                    if (searchModel.SalerPermission == null || searchModel.SalerPermission.Trim() == "")
                                    {
                                        searchModel.SalerPermission = current_user.UserUnderList;
                                    }
                                    else
                                    {
                                        searchModel.SalerPermission += "," + current_user.UserUnderList;

                                    }
                                }
                                break;
                            case (int)RoleType.Admin:
                            case (int)RoleType.KT:
                            case (int)RoleType.GD:
                            case (int)RoleType.PhoTPKeToan:
                                {
                                    searchModel.Sale = null;
                                }
                                break;
                        }
                    }
                    searchModel.PermisionType = null;
                    searchModel.PaymentStatus = null;
                    searchModel.BoongKingCode = null;

                    var dataOrder = await _OrderRepository.GetTotalCountOrder(searchModel, 1, 10);
                    return new JsonResult(dataOrder);
                }
                else
                {
                    return new JsonResult(new
                    {
                        totalrecordErr = 0
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetOrderStatistic - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }

        [HttpPost]
        public IActionResult SumAmountTransactionSMs(TransferSmsSearchModel searchModel)
        {
            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                long total = 0;
                var totalAmountTienVao = transferSmsService.SumAmountTransactionSMs(searchModel);
                return new JsonResult(totalAmountTienVao);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("SumAmountTransactionSMs - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }
        [HttpPost]
        public IActionResult GetTotalAmountPaymentVoucherByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                long total = 0;
                var totalAmountTienVao = transferSmsService.SumAmountTransactionSMs(searchModel);
                var searchModel2 = searchModel;
                searchModel2.type = 2;
                var totalAmountTienRa = transferSmsService.SumAmountTransactionSMs(searchModel2);
              
                var data = new
                {
                    totalAmountTransactionSMs = totalAmountTienVao,
                    totalAmountPayment = 0 - totalAmountTienRa.Amount,
                };
                return new JsonResult(data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetTotalAmountPaymentVoucherByDate - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }
        [HttpPost]
        public IActionResult GetCountAmountPaymentVoucherByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                long total = 0;
                searchModel.type = 2;
                var count = transferSmsService.SumAmountTransactionSMs(searchModel); ;
                return new JsonResult(count.total);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetCountAmountPaymentVoucherByDate - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }
        [HttpPost]
        public IActionResult GetListTransferSms(TransferSmsSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {

            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                long total = 0;
                var listTransactionSms = transferSmsService.SearchTransactionSMs(searchModel, out total, currentPage, pageSize);
                foreach(var item in listTransactionSms)
                {
                    var BankOnePay = _allCodeRepository.GetBankOnePayByBankName(item.BankName);
                    if (BankOnePay != null)
                        item.logo = BankOnePay.Logo;
                }
                return new JsonResult(listTransactionSms);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTransferSms - DashBoardController: " + ex.ToString());
            }
            return Content("");
        }
        [HttpPost]
        public IActionResult GetListTransferSmsGroupByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                var list_data = new List<TransferSmsTotalModel>();
                long total = 0;
                var total2 = DateTime.Now.Month;
                var ListTransactionSMs = transferSmsService.ListTransactionSMs(searchModel);
                for (var item = 1; item <= 12; item++)
                {
                    var data = new TransferSmsTotalModel();
                    data.AmountTransaction = ListTransactionSMs.Where(s => s.Amount > 0 && Convert.ToDateTime(s.ReceiveTime).Month == item).Sum(s => s.Amount);
                    data.Amount = 0 - ListTransactionSMs.Where(s => s.Amount < 0 && Convert.ToDateTime(s.ReceiveTime).Month == item).Sum(s => s.Amount);
                    data.Balance = data.AmountTransaction - data.Amount;
                    data.Month = item;
                    if(data.AmountTransaction !=0 || data.Amount != 0 || data.Balance != 0)
                    list_data.Add(data);
                }


                return new JsonResult(list_data);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetListTransferSmsGroupByDate - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }
        [HttpPost]
        public async Task<IActionResult> GettotalAmountBankingAccountTransferSmsGroupByDate(TransferSmsSearchModel searchModel)
        {
            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                long total = 0;
                double totalAmount = 0;
                //var totalAmount = transferSmsService.SumAmountBankingAccountTransactionSMs(searchModel);
                var newDate = DateTime.Now;

                var ListBankingAccountTransactionSMs = transferSmsService.GetLisstBankingAccountTransactionSMs();
                foreach (var item in ListBankingAccountTransactionSMs)
                {
                    searchModel.AccountNumber = item.AccountNumber;
                    searchModel.BankName = item.BankId;
                    var SumTotalAmountTransactionSMs =await transferSmsService.SumTotalAmountTransactionSMs(searchModel.AccountNumber, searchModel.BankName, newDate);
                    //Sum_Total_Amount_TransactionSMs += SumTotalAmountTransactionSMs;
                    item.TransactionSMs_Amount = SumTotalAmountTransactionSMs;
                }
                totalAmount = ListBankingAccountTransactionSMs.Sum(s=>s.TransactionSMs_Amount) + ListBankingAccountTransactionSMs.Sum(s => s.Amount);
                return new JsonResult(totalAmount);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GettotalAmountBankingAccountTransferSmsGroupByDate - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }
        [HttpPost]
        public IActionResult ListBankingAccountTransactionSMs(TransferSmsSearchModel searchModel)
        {
            try
            {
                TransferSmsService transferSmsService = new TransferSmsService();
                long total = 0;
                var ListAC = transferSmsService.GetLisstBankingAccountTransactionSMs();



                return new JsonResult(ListAC);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ListBankingAccountTransactionSMs - DashBoardController: " + ex.ToString());
                return Content("");
            }
        }

    }
}