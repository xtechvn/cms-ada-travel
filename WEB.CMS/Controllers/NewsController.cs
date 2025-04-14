using APP_CHECKOUT.RabitMQ;
<<<<<<< HEAD
using Entities.ConfigModels;
=======
>>>>>>> main
using Entities.Models;
using Entities.ViewModels;
using Entities.ViewModels.News;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Repositories.IRepositories;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Service.News;

namespace WEB.CMS.Controllers
{
    [CustomAuthorize]
    public class NewsController : Controller
    {
        private const int NEWS_CATEGORY_ID = 39;
        private const int VIDEO_NEWS_CATEGORY_ID = 36;
        private readonly IGroupProductRepository _GroupProductRepository;
        private readonly IArticleRepository _ArticleRepository;
        private readonly IUserRepository _UserRepository;
        private readonly ICommonRepository _CommonRepository;
        private readonly IWebHostEnvironment _WebHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly WorkQueueClient workQueueClient;
        private readonly string _UrlStaticImage;

        public NewsController(IConfiguration configuration, IArticleRepository articleRepository, IUserRepository userRepository, ICommonRepository commonRepository, IWebHostEnvironment hostEnvironment,
            IGroupProductRepository groupProductRepository, IOptions<DomainConfig> domainConfig)
        {
            _ArticleRepository = articleRepository;
            _CommonRepository = commonRepository;
            _UserRepository = userRepository;
            _WebHostEnvironment = hostEnvironment;
            _configuration = configuration;
            _GroupProductRepository = groupProductRepository;
            _UrlStaticImage = domainConfig.Value.ImageStatic;
            workQueueClient = new WorkQueueClient(configuration);


        }

        public async Task<IActionResult> Index()
        {
            ViewBag.ListArticleStatus = await _CommonRepository.GetAllCodeByType(AllCodeType.ARTICLE_STATUS);
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1);
            ViewBag.ListAuthor = await _UserRepository.GetUserSuggestionList(string.Empty);
            return View();
        }

        /// <summary>
        /// Search News
        /// </summary>
        /// <param name="searchModel"></param>
        /// <param name="currentPage"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpPost]
        public IActionResult Search(ArticleSearchModel searchModel, int currentPage = 1, int pageSize = 20)
        {
            var model = new GenericViewModel<ArticleViewModel>();
            try
            {
                model = _ArticleRepository.GetPagingList(searchModel, currentPage, pageSize);
                ViewBag.ListID = (model!=null&&model.ListData!=null&& model.ListData.Select(x => x.Id).ToList() != null && model.ListData.Select(x => x.Id).ToList().Count>0) ? JsonConvert.SerializeObject(model.ListData.Select(x => x.Id).ToList()) : "";
            }
            catch
            {

            }
            return PartialView(model);
        }

        public async Task<IActionResult> Detail(long Id)
        {
            var model = new ArticleModel();
            if (Id > 0)
            {
                model = await _ArticleRepository.GetArticleDetail(Id);
            }
            else
            {
                model.Status = ArticleStatus.SAVE;
            }
            ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1, model.Categories);
            return View(model);
        }
        


        public async Task<string> GetSuggestionTag(string name)
        {
            try
            {
                var tagList = await _ArticleRepository.GetSuggestionTag(name);
                return JsonConvert.SerializeObject(tagList);
            }
            catch
            {
                return null;
            }
        }

        public async Task<IActionResult> RelationArticle(long Id)
        {
            //ViewBag.StringTreeViewCate = await _GroupProductRepository.GetListTreeViewCheckBox(NEWS_CATEGORY_ID, -1);
            ViewBag.ListAuthor = await _UserRepository.GetUserSuggestionList(string.Empty);
            return PartialView();
        }

        [HttpPost]
        public IActionResult RelationSearch(ArticleSearchModel searchModel, int currentPage = 1, int pageSize = 10)
        {
            var model = new GenericViewModel<ArticleViewModel>();
            try
            {
                model = _ArticleRepository.GetPagingList(searchModel, currentPage, pageSize);
            }
            catch
            {

            }
            return PartialView(model);
        }
        public async Task<IActionResult> AddOrUpdate(int id, int parent_id)
        {
            var model = new ArticleViewModel
            {
               
                Status = 0
            };

            if (id > 0)
            {
                var article = await _ArticleRepository.GetArticleDetail(id);
                model = new ArticleViewModel
                {
                    Id = article.Id,
                    Status = article.Status,
                    CampaignName = article.CampaignName,
                    AiContent = article.AiContent,
                    PlatForm = article.PlatForm,
                    AimodelType = article.AimodelType
                };
            }
            return View(model);
        }

        //[HttpPost]
        //public async Task<IActionResult> AddOrUpdate(MenuUpsertViewModel model)
        //{
        //    try
        //    {
        //        long result = 0;
        //        string ActionName = string.Empty;
        //        if (model.Id > 0)
        //        {
        //            ActionName = "Cập nhật";
        //            result = await _MenuRepository.Update(model);
        //        }
        //        else
        //        {
        //            ActionName = "Thêm mới";
        //            result = await _MenuRepository.Create(model);
        //        }

        //        if (result > 0)
        //        {
        //            return new JsonResult(new
        //            {
        //                isSuccess = true,
        //                message = $"{ActionName} menu thành công"
        //            });
        //        }
        //        else
        //        {
        //            return new JsonResult(new
        //            {
        //                isSuccess = false,
        //                message = $"{ActionName} menu thất bại"
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new JsonResult(new
        //        {
        //            isSuccess = false,
        //            message = ex.Message
        //        });
        //    }
        //}
        [HttpGet]
        public async Task<IActionResult> GetFanpageImages(long articleId)
        {
            var images = await _ArticleRepository.GetFanpageImagesAsync(articleId);
            return Json(images);
        }


        [HttpPost]
        public async Task<IActionResult> SaveFanpageImages([FromBody] FanpageImageSaveModel model)
        {
            if (model.ArticleId <= 0 || model.Images == null || !model.Images.Any())
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            await _ArticleRepository.SaveFanpageImagesAsync(model.ArticleId, model.Images);

            return Json(new { success = true });
        }
<<<<<<< HEAD
        [HttpPost]
        public async Task<IActionResult> ConvertImagesBeforePost([FromBody] List<string> images)
        {
            var processedImages = new List<string>();

            foreach (var image in images)
            {
                var url = await UpLoadHelper.UploadBase64Src(image, _UrlStaticImage);
                // ✅ Nếu URL trả về KHÔNG chứa static domain thì gắn vào
                if (!string.IsNullOrEmpty(url) && !url.Contains(_UrlStaticImage))
                {
                    url = _UrlStaticImage + url;
                }
                if (!string.IsNullOrEmpty(url))
                {
                    processedImages.Add(url);
                }
            }

            return Ok(processedImages);
        }
=======
>>>>>>> main


        [HttpPost]
        public async Task<IActionResult> UpSert([FromBody] object data)
        {
            try
            {

                
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };

                var model = JsonConvert.DeserializeObject<ArticleModel>(data.ToString(), settings);
                // Nếu là bài viết tạo từ AI để đăng Fanpage thì chỉ cần nội dung (Body)
                bool isFanpageAi = model.AimodelType == 1 && model.PlatForm == 1;


                if (await _GroupProductRepository.IsGroupHeader(model.Categories)) model.Categories.Add(NEWS_CATEGORY_ID);

                if (model != null && HttpContext.User.FindFirst(ClaimTypes.NameIdentifier) != null)
                {
                    model.AuthorId = int.Parse(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);
                }

                // Làm nổi bật link trong nội dung
                model.Body = ArticleHelper.HighLightLinkTag(model.Body);
                // Nếu là AI Fanpage, gán mặc định cho Title và Lead nếu bị null
                if (isFanpageAi)
                {
                    model.Categories = null ; 
                    model.Title = model.CampaignName;
                    model.Lead = model.AiContent;
                }


                // ✅ Kiểm tra điều kiện bắt buộc
                if (string.IsNullOrWhiteSpace(model.Body))
                {
                    return Json(new
                    {
                        isSuccess = false,
                        message = "Phần nội dung bài viết không được để trống"
                    });
                }
                // ✅ Nếu KHÔNG phải Fanpage-AI → kiểm tra thêm tiêu đề và mô tả
                if (!isFanpageAi)
                {
                    if (string.IsNullOrWhiteSpace(model.Title) || string.IsNullOrWhiteSpace(model.Lead))
                    {
                        return Json(new
                        {
                            isSuccess = false,
                            message = "Phần Tiêu đề và Mô tả không được để trống"
                        });
                    }

                    if (model.Lead.Length > 400)
                    {
                        return Json(new
                        {
                            isSuccess = false,
                            message = "Phần Mô tả không được vượt quá 400 ký tự"
                        });
                    }
                }
                var articleId = await _ArticleRepository.SaveArticle(model);

                if (articleId > 0)
                {
                    // clear cache article
                    var strCategories = string.Empty;
                    if (model.Categories != null && model.Categories.Count > 0)
                        strCategories = string.Join(",", model.Categories);

                    await ClearCacheArticle(articleId, strCategories);
                    workQueueClient.SyncES(articleId, _configuration["DataBaseConfig:Elastic:SP:sp_GetArticle"], _configuration["DataBaseConfig:Elastic:Index:Article"], ProjectType.ADAVIGO_CMS);

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Cập nhật thành công",
                        dataId = articleId
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Cập nhật thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("UpSert - NewsController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> ChangeArticleStatus(long Id, int articleStatus)
        {
            try
            {
                var _ActionName = string.Empty;

                switch (articleStatus)
                {
                    case ArticleStatus.PUBLISH:
                        _ActionName = "Đăng bài viết";
                        break;

                    case ArticleStatus.REMOVE:
                        _ActionName = "Hạ bài viết";
                        break;
                }

                var rs = await _ArticleRepository.ChangeArticleStatus(Id, articleStatus);

                if (rs > 0)
                {
                    //  clear cache article
                    var Categories = await _ArticleRepository.GetArticleCategoryIdList(Id);
                    await ClearCacheArticle(Id, string.Join(",", Categories));

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = _ActionName + " thành công",
                        dataId = Id
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = _ActionName + " thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ChangeArticleStatus - NewsController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task<IActionResult> DeleteArticle(long Id)
        {
            try
            {
                var Categories = await _ArticleRepository.GetArticleCategoryIdList(Id);
                var rs = await _ArticleRepository.DeleteArticle(Id);

                if (rs > 0)
                {
                    //  clear cache article
                    await ClearCacheArticle(Id, string.Join(",", Categories));

                    return new JsonResult(new
                    {
                        isSuccess = true,
                        message = "Xóa bài viết thành công",
                        dataId = Id
                    });
                }
                else
                {
                    return new JsonResult(new
                    {
                        isSuccess = false,
                        message = "Xóa bài viết thất bại"
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("DeleteArticle - NewsController: " + ex);
                return new JsonResult(new
                {
                    isSuccess = false,
                    message = ex.Message.ToString()
                });
            }
        }

        public async Task ClearCacheArticle(long articleId, string ArrCategoryId)
        {
            string token = string.Empty;
            try
            {
                var apiPrefix = ReadFile.LoadConfig().API_ADAVIGO_URL + ReadFile.LoadConfig().API_SYNC_ARTICLE;
                var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                HttpClient httpClient = new HttpClient();
                var j_param = new Dictionary<string, string> {
                    { "article_id", articleId.ToString() },
                    { "category_id",ArrCategoryId }
                };
                token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), _configuration["DataBaseConfig:key_api:api_manual"]);
                var content = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("token", token) });
                var result_post=await httpClient.PostAsync(apiPrefix, content);
                var post_content =JObject.Parse(result_post.Content.ReadAsStringAsync().Result);
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("ClearCacheArticle - " + ex.ToString() + " Token:" + token);
            }
        }
        [HttpPost]
        public  async Task<List<NewsViewCount>> GetPageViewByList(List<long> article_id)
        {
            try
            {
                NewsMongoService news_services = new NewsMongoService(_configuration);
                return await news_services.GetListViewedArticle(article_id);
            }
            catch
            {

            }
            return null;        
        }
        
    }
}
