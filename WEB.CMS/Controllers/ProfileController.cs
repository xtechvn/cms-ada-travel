using Entities.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class ProfileController : Controller
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IAttachFileRepository _attachFileRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileController(IDocumentRepository documentRepository, IAttachFileRepository attachFileRepository, IWebHostEnvironment webHostEnvironment)
        {
            _documentRepository = documentRepository;
            _attachFileRepository = attachFileRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Search(string name, int status, int pageIndex = 1, int pageSize = 10)
        {
            var data = await _documentRepository.GetList(name, status, pageIndex, pageSize);
            var totalCount = await _documentRepository.GetCount(name, status);
            ViewBag.TotalCount = totalCount;
            ViewBag.PageIndex = pageIndex;
            ViewBag.PageSize = pageSize;
            return PartialView(data);
        }

        public async Task<IActionResult> Upsert(int id)
        {
            var model = new Document();
            if (id > 0)
            {
                model = await _documentRepository.GetById(id);
            }
            ViewBag.Type = (int)AttachmentType.Document;
            return PartialView(model);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Document model, string filePdf, int isSendApproval)
        {
            try
            {
                var userLogin = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                if (model.Id == 0)
                {
                    if (!string.IsNullOrEmpty(filePdf) && !filePdf.Contains("https://static-image.adavigo.com"))
                    {
                        filePdf = "https://static-image.adavigo.com" + (filePdf.StartsWith("/") ? "" : "/") + filePdf;
                    }
                    model.CreatedBy = userLogin.ToString();
                    model.CreatedDate = DateTime.Now;
                    model.Status = (byte)(isSendApproval == 1 ? 1 : 0);
                    model.FilePath = filePdf;
                    model.IsDeleted = false;
                    var result = await _documentRepository.Insert(model);
                    if (result > 0)
                    {
                        return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Thành công", data = result });
                    }
                    else
                    {
                        return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Tạo mới không Thành công" });

                    }
                }
                else
                {
                    var existing = await _documentRepository.GetById(model.Id);
                    if (existing != null)
                    {
                        if (!string.IsNullOrEmpty(filePdf) && !filePdf.Contains("https://static-image.adavigo.com"))
                        {
                            filePdf = "https://static-image.adavigo.com" + (filePdf.StartsWith("/") ? "" : "/") + filePdf;
                        }

                        // In Step 3: if rejected (status 2), only allow re-upload PDF
                        if (existing.Status == 2)
                        {
                            existing.FilePath = filePdf;
                            existing.Status = (byte)(isSendApproval == 1 ? 1 : 0);
                        }
                        else
                        {
                            existing.DocumentName = model.DocumentName;
                            existing.Category = model.Category;
                            existing.Description = model.Description;
                            existing.FilePath = filePdf;
                            existing.Status = (byte)(isSendApproval == 1 ? 1 : 0);
                        }
                        var result = await _documentRepository.Update(existing);
                        if (result > 0)
                        {
                            return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Thành công", data = existing.Id });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Save - ProfileController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.FAILED, msg = "Thất bại" });
        }

        public async Task<IActionResult> Detail(int id)
        {
            var model = await _documentRepository.GetById(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(int id, string signatureBase64)
        {
            try
            {
                var doc = await _documentRepository.GetById(id);
                if (doc != null)
                {
                    // 1. Download existing PDF
                    byte[] pdfBytes;
                    using (var httpClient = new HttpClient())
                    {
                        if (!string.IsNullOrEmpty(doc.FilePath) && !doc.FilePath.Contains("https://static-image.adavigo.com"))
                        {
                            doc.FilePath = "https://static-image.adavigo.com" + doc.FilePath;
                        }
                        pdfBytes = await httpClient.GetByteArrayAsync(doc.FilePath);
                    }

                    // 2. Merge signature into PDF
                    var signedPdfBytes = PdfHelper.SignPdf(pdfBytes, signatureBase64);
                    if (signedPdfBytes == null)
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Lỗi khi ký PDF" });
                    }

                    // 3. Upload signed PDF
                    string fileName = "Signed_" + (doc.FilePath.Contains("/") ? doc.FilePath.Split('/')[doc.FilePath.Split('/').Length - 1] : "document.pdf");
                    string newFilePath = await UpLoadHelper.UploadFileByBytes(signedPdfBytes, fileName, doc.Id, 9);

                    if (string.IsNullOrEmpty(newFilePath))
                    {
                        return Ok(new { status = (int)ResponseType.FAILED, msg = "Lỗi khi upload file đã ký" });
                    }

                    // 4. Update status to 3 (Completed)
                    doc.Status = 3;
                    doc.ImgLocation = newFilePath;
                    var result = await _documentRepository.Update(doc);
                    
                    return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Duyệt và ký thành công", path = newFilePath });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Approve - ProfileController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.FAILED, msg = "Thất bại" });
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int id, string reason)
        {
            try
            {
                var doc = await _documentRepository.GetById(id);
                if (doc != null)
                {
                    doc.Status = 2;
                    doc.Notes = reason;
                    var result = await _documentRepository.Update(doc);
                    return Ok(new { status = (int)ResponseType.SUCCESS, msg = "Từ chối thành công" });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Reject - ProfileController: " + ex);
            }
            return Ok(new { status = (int)ResponseType.FAILED, msg = "Thất bại" });
        }
        [CustomAuthorize]
        [HttpPost]
        public async Task<IActionResult> ApprovePopup(int id)
        {
            var model =await _documentRepository.GetById(id);
            return PartialView(model);
        }

        [CustomAuthorize]
        [HttpPost]
        public async Task<IActionResult> RejectPopup(int id)
        {
            var model =await _documentRepository.GetById(id);
            return PartialView(model);
        }
    }
}
