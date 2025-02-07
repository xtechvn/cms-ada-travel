using Entities.ViewModels.Department;
using Microsoft.AspNetCore.Mvc;
using Repositories.IRepositories;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities.Contants;
using WEB.CMS.Customize;

namespace WEB.DeepSeekTravel.CMS.Controllers.Configs
{
    [CustomAuthorize]

    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _DepartmentRepository;
        private readonly IAllCodeRepository _allCodeRepository;
       

        public DepartmentController(IDepartmentRepository departmentRepository, IAllCodeRepository allCodeRepository)
        {
            _DepartmentRepository = departmentRepository;
            _allCodeRepository = allCodeRepository;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Search(string name)
        {
            int? tenant_id = null;
            if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
            {
                try
                {
                    tenant_id = Convert.ToInt32(HttpContext.User.FindFirst("TenantId").Value);
                }
                catch { }
                if (tenant_id <= 0) tenant_id = null;
            }
            var departments = await _DepartmentRepository.Listing(name,tenant_id);
            return View(departments);
        }

        public async Task<IActionResult> AddOrUpdate(int id, int parent_id)
        {
            var model = new DepartmentViewModel
            {
                ParentId = parent_id,
                IsDelete = false
            };

            if (id > 0)
            {
                var department = await _DepartmentRepository.GetById(id);
                model = new DepartmentViewModel
                {
                    Id = id,
                    ParentId = department.ParentId,
                    Description = department.Description,
                    DepartmentCode = department.DepartmentCode,
                    DepartmentName = department.DepartmentName,
                    Branch = department.Branch
                };
            }
            ViewBag.Branch = _allCodeRepository.GetListByType(AllCodeType.BRANCH_CODE);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddOrUpdate(DepartmentViewModel model)
        {
            try
            {
                long result = 0;
                string action = string.Empty;
                int? tenant_id = null;
                if (HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    try
                    {
                        tenant_id = Convert.ToInt32(HttpContext.User.FindFirst("TenantId").Value);
                    }
                    catch { }
                    if (tenant_id <= 0) tenant_id = null;
                }
                model.TenantId = tenant_id;
                if (model.Id > 0)
                {
                    action = "Cập nhật";
                    result = await _DepartmentRepository.Update(model);
                }
                else
                {
                    action = "Thêm mới";
                    result = await _DepartmentRepository.Create(model);
                }

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = $"{action} phòng ban thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = $"{action} phòng ban thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }

        public async Task<IActionResult> Delete(int Id)
        {
            try
            {
                var result = await _DepartmentRepository.Delete(Id);

                if (result > 0)
                {
                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa phòng ban thành công"
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa phòng ban thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message
                });
            }
        }
    }
}
