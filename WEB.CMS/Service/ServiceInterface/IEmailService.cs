using Entities.Models;
using Entities.ViewModels.Attachment;
using Entities.ViewModels.HotelBookingCode;
using Entities.ViewModels.VinWonder;
using Microsoft.Extensions.Configuration;
using Nest;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEB.Adavigo.CMS.Service.ServiceInterface
{
    public interface IEmailService
    {
        Task<bool> SendEmail(SendEmailViewModel model, List<AttachfileViewModel> attach_file);
        Task<string> GetTemplateinsertUser(SendEmailViewModel model, long id, string order_note = "", string payment_notification = "", bool is_edit_form = false);
        Task<string> GetFlyBookingTemplateBody(SendEmailViewModel model, string group_booking_id, string order_note = "", string payment_notification = "", bool is_edit_form = false);
        Task<string> TourTemplateBody(SendEmailViewModel model, long id, string order_note = "", string payment_notification = "", bool is_edit_form = false);
        Task<string> OrderTemplateBody(SendEmailViewModel model, long id, string payment_notification = "", bool is_edit_form = false);
        Task<string> GetOrderFlyTemplateBody(long id, string order_note = "", string payment_notification = "", bool is_edit_form = false);
        Task<string> GetTemplateSupplier(SendEmailViewModel modelEmail, long id, long SupplierId,long type ,string order_note = "", string payment_notification = "", bool is_edit_form = false);
        Task<string> OrderTemplateSaleDH(long id, string payment_notification = "", bool is_edit_form = false);
        Task<string> TemplatePaymentRequest(long id,string profit,int type,EmailYCChiViewModel Model);
        Task<string> PathAttachmentVeVinWonder(VinWonderConfirmBookingOutputDataDataTickets Url);
        Task<string> UploadImageBase64(string base64, string extension, string file_path);
        Task<string> GetTemplateVinWordbookingTC(long orderid);
        Task<bool> SendEmailVinwonderTicket(long orderid, long booking_id, string subject, List<string> to_email, List<string> cc_email, List<string> bcc_email);
        Task<string> GetTemplatePaymentVoucher(int paymentVoucherId);
        Task<bool> SendEmailpaymentVoucher(int paymentVoucherId, List<AttachFile> attach_file, List<string> CC_Email, List<string> BCC_Email, string Email, string To_Email, string subject_name);
        Task<string> GetTemplateHotelBookingCode(long ServiceId, long OrderId,string Note, string Description);
        Task<bool> SendEmailBookingCode(long Id, long OrderId, List<string> CC_Email, List<string> BCC_Email, string Email, string To_Email, string subject_name, string Note, string Description);
    }
}
