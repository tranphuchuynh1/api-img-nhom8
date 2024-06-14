using Microsoft.AspNetCore.Mvc;
using MVCImage.Services;
using MVCImage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace MVCImage.Controllers
{

    public class ImagesController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<ImagesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public ImagesController(ApiService apiService, ILogger<ImagesController> logger, IHttpClientFactory httpClientFactory)
        {
            _apiService = apiService;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        private void AddAuthorizationHeader(HttpClient client)
        {
            var token = HttpContext.Session.GetString("AccessToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        // GET: Images
       // [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var images = await _apiService.GetImagesAsync();
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            return View(images);
        }

        // GET: Images/Details/5
      //  [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var image = await _apiService.GetImageByIdAsync(id.Value);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // GET: Images/Create
       // [Authorize(Roles = "Write")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Images/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
      //  [Authorize(Roles = "Write")]
        public async Task<IActionResult> Create([Bind("Title,Description,ImageUrl,CategoryId")] ImageUploadDto imageDto)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                await _apiService.CreateImageAsync(imageDto);
                await _apiService.LogHistoryAsync("Create", "Image", 0, $"Đã tạo ảnh có tiêu đề: {imageDto.Title}");
                TempData["SuccessMessage"] = "Hình ảnh được tạo thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(imageDto);
        }

        // GET: Images/Edit/5
       // [Authorize(Roles = "Write")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Edit method called with null id");
                return NotFound();
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var image = await _apiService.GetImageByIdAsync(id.Value);
            if (image == null)
            {
                _logger.LogWarning("Image not found with id {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("Edit method retrieved image with id {Id}", id);
            return View(image);
        }

        // POST: Images/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
       // [Authorize(Roles = "Write")]
        public async Task<IActionResult> Edit(int id, [Bind("ImageId,Title,Description,ImageUrl,CategoryId")] Image image)
        {
            if (id != image.ImageId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                await _apiService.UpdateImageAsync(id, image);
                await _apiService.LogHistoryAsync("Edit", "Image", id, $"Đã sửa ảnh có tiêu đề: {image.Title}");
                TempData["SuccessMessage"] = "Đã cập nhật hình ảnh thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(image);
        }

        // GET: Images/Delete/5
        //[Authorize(Roles = "Write")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var image = await _apiService.GetImageByIdAsync(id.Value);
            if (image == null)
            {
                return NotFound();
            }

            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
      //  [Authorize(Roles = "Write")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var image = await _apiService.GetImageByIdAsync(id); // Lấy thông tin của hình ảnh
            if (image == null)
            {
                return NotFound();
            }

            await _apiService.DeleteImageAsync(id);
            await _apiService.LogHistoryAsync("Delete", "Image", id, $"Đã xóa ảnh có tiêu đề: {image.Title}");
            TempData["SuccessMessage"] = "Đã xóa hình ảnh thành công.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Images/Search
       // [Authorize(Roles = "Read,Write")]
        public IActionResult Search()
        {
            return View(new List<Image>());
        }

        [HttpPost]
       // [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> Search(string title, string description)
        {
            if (string.IsNullOrWhiteSpace(title) && string.IsNullOrWhiteSpace(description))
            {
                TempData["ErrorMessage"] = "Yêu cầu nhập nội dung để tìm kiếm.";
                return View(new List<Image>());
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var images = await _apiService.SearchImagesAsync(title, description);
            if (images.Any())
            {
                TempData["SuccessMessage"] = "Tìm kiếm thành công.";
                string details = $"Tìm kiếm hình ảnh có tiêu đề: '{title}' và mô tả: '{description}'";
                await _apiService.LogHistoryAsync("Search", "Image", 0, details); // Ghi lịch sử tìm kiếm với thông tin chi tiết
            }
            else
            {
                TempData["ErrorMessage"] = "Không tìm thấy hình ảnh phù hợp.";
            }
            return View(images);
        }
    }
}
