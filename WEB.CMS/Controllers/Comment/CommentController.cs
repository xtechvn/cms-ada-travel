using Caching.RedisWorker;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using System.Collections.Concurrent;
using Utilities.Contants;
using Utilities;
using WEB.CMS.Service;
using System.Security.Claims;

namespace WEB.CMS.Controllers.Comment
{
    public class CommentController : Controller
    {
        private readonly ISubscriber _subscriber;
        private readonly RedisConn _redisService;
        private readonly IConfiguration _configuration;
        private readonly CommentService _commentService;

        public CommentController(RedisConn redisService, IConfiguration configuration, CommentService commentService)
        {
            _redisService = redisService;
            _configuration = configuration;

            var connection = ConnectionMultiplexer.Connect(_configuration["Redis:Host"] + ":" + _configuration["Redis:Port"]);
            _subscriber = connection.GetSubscriber();
            _commentService = commentService;
        }

        // Phương thức lắng nghe comment mới qua SSE
        [HttpGet]
        public async Task GetCommentsStream(long requestId)
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            var dataQueue = new ConcurrentQueue<string>();

            _subscriber.Subscribe("COMMENT_" + requestId, (channel, message) =>
            {
                dataQueue.Enqueue(message);
            });

            while (!HttpContext.RequestAborted.IsCancellationRequested)
            {
                while (dataQueue.TryDequeue(out var message))
                {
                    var data = $"data: {message}\n\n";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(data);
                    await Response.Body.WriteAsync(buffer, 0, buffer.Length);
                    await Response.Body.FlushAsync();
                }
                await Task.Delay(100);
            }
        }

        //// Load danh sách comment ban đầu
        [HttpPost]
        public async Task<IActionResult> LoadComments(int requestId)
        {
            try
            {
                var comments = await _commentService.GetListCommentsAsync(requestId);

                return Ok(new
                {
                    status = comments != null ? 1 : 0,
                    data = comments
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"LoadComments Error: {ex}");
                return Ok(new { status = 0, msg = "Failed to load comments." });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddComment([FromForm] int requestId, [FromForm] string content, [FromForm] List<IFormFile> attachFiles)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content) && (attachFiles == null || !attachFiles.Any()))
                {
                    return BadRequest(new { status = 1, msg = "Vui lòng nhập nội dung hoặc đính kèm file." });
                }
                int userid = Convert.ToInt32(HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value);

             

                var type = 0;

                var createdBy = userid;
                var fileUrls = new List<string>();
                var fileNames = new List<string>();

                // Kiểm tra file và upload
                if (attachFiles != null && attachFiles.Any())
                {
                    long totalSize = attachFiles.Sum(f => f.Length);
                    if (totalSize > 25 * 1024 * 1024) // 25MB
                    {
                        return BadRequest(new { status = 1, msg = "Tổng dung lượng file vượt quá 25MB." });
                    }

                    foreach (var file in attachFiles)
                    {
                        var fileUrl = await UpLoadHelper.UploadFileOrImage(file, requestId, 35);
                        if (!string.IsNullOrEmpty(fileUrl))
                        {
                            fileUrls.Add(fileUrl);
                            fileNames.Add(file.FileName);
                        }
                    }
                }

                // Lưu comment vào database
                var newComment = await _commentService.AddCommentAsync(requestId, userid, type, content, fileUrls, fileNames, createdBy);

                if (newComment != null)
                {
                    return Ok(new { status = 0, msg = "Comment added successfully.", data = newComment });
                }

                return BadRequest(new { status = 1, msg = "Failed to add comment." });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram($"AddComment Error: {ex}");
                return StatusCode(500, new { status = 1, msg = "Internal server error." });
            }
        }




    }
}
