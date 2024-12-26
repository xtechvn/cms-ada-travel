using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Nest;
using PdfSharp.Pdf.Content.Objects;
using Repositories.IRepositories;
using System.Linq;
using System.Security.Claims;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class AllCodeController : Controller
    {
        private readonly IAllCodeRepository _allCodeRepository;
        public AllCodeController(IAllCodeRepository allCodeRepository)
        {
            _allCodeRepository = allCodeRepository;
        }
        public IActionResult Index()
        {
            var data= _allCodeRepository.GetAll();
            ViewBag.data= data.GroupBy(s=>s.Type).Select(i => i.First()).ToList();
            return View();
        } 
        public IActionResult Search(string type)
        {
            var data = _allCodeRepository.GetListByType(type);
            return View(data);
        }
        public async Task<IActionResult> AddOrUpdate(int id)
        {
            ViewBag.id = id;
            if (id != 0)
            {
                var data =await _allCodeRepository.GetById(id);
                return View(data);
            }
            return View();
        }
        public async Task<IActionResult> Delete(int id)
        {
            ViewBag.id = id;
            if (id != 0)
            {
                var data =await _allCodeRepository.Delete(id);
                return View(data);
            }
            return View();
        }
        public async Task<IActionResult> SetUp(AllCode model)
        {
            int user_id = 0;
            try
            {
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    user_id = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }
                if (model != null && model.Id == 0)
                {
                    model.CreatedBy = user_id;
                    var Create = await _allCodeRepository.InsertAllcode(model);
                    if(Create > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Thêm mới thành công",

                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = "Thêm mới không thành công",

                        });
                    }
                  
                }
                else
                {
                    model.UpdatedBy = user_id;
                    var update =await _allCodeRepository.UpdateAllCode(model);
                  
                    if (update > 0)
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.SUCCESS,
                            msg = "Cập nhật thành công",

                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            status = (int)ResponseType.ERROR,
                            msg = "Cập nhật không thành công",

                        });
                    }
                }
            }
            catch (Exception ex) {
                return Ok(new
                {
                    status = (int)ResponseType.ERROR,
                    msg = "Thêm mới / Cập nhật không thành công",
                });
            }
        }
    }
}
