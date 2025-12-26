using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.Apartment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IApartmentOrderRepository
    {
        /// <summary>
        /// Gợi ý danh sách Căn hộ (Hotel.IsApartment = 1) cho select2.
        /// </summary>
        /// <param name="term">Từ khóa tìm kiếm theo tên căn</param>
        /// <param name="size">Số lượng tối đa</param>
        /// <returns>Danh sách căn hộ gợi ý</returns>
        Task<List<ApartmentSuggestViewModel>> Suggest(string term, int size = 20);
        Task<GenericViewModel<OrderViewModel>> GetTotalCountOrder(OrderViewSearchModel searchModel, int currentPage, int pageSize);
        Task<GenericViewModel<OrderViewModel>> GetList(OrderViewSearchModel searchModel, int currentPage, int pageSize);
        Task<List<ApartmentRoomLedger>> LoadLedgerPopup(int roomId);
        List<ReportHotelShareHolderViewModel> GetReportHotelShareHolder(
    ReportHotelShareHolderSearchModel model,
    out long total);

        List<HotelShareHolderPaymentGridModel> GetListPayment(string name, int pageIndex, int pageSize, out long total);
             int InsertPayment(HotelShareHolderPayment model);
        int DeletePayment(int id);

        List<ShareHolderSearchViewModel> SearchShareHolder(string keyword);
        List<ReportHotelShareHolderDetailViewModel>
    GetShareHolderDetail(int shareHolderId);


        Task<int> SaveLedger(RoomLedgerInput model, int userId);

        ///// <summary>
        ///// Tạo đơn căn hộ mới.
        ///// </summary>
        ///// <param name="model">Thông tin đơn căn hộ</param>
        ///// <param name="currentUserId">Id user đang thao tác</param>
        ///// <returns>Id đơn vừa tạo, -1 nếu lỗi</returns>
        //Task<int> Create(ApartmentOrderCreateViewModel model, int currentUserId);
    }
}
