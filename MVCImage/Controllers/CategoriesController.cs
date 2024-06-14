using Microsoft.AspNetCore.Mvc;
using MVCImage.Services;
using MVCImage.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using System.Net.Http.Headers;

namespace MVCImage.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<CategoriesController> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public CategoriesController(ApiService apiService, ILogger<CategoriesController> logger, IHttpClientFactory httpClientFactory)
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

        // GET: Categories
      //  [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var categories = await _apiService.GetCategoriesAsync();
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            return View(categories);
        }

        // GET: Categories/Details/5
       // [Authorize(Roles = "Read,Write")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var category = await _apiService.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        //[Authorize(Roles = "Write")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Name,Description")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var client = _httpClientFactory.CreateClient();
                    AddAuthorizationHeader(client);
                    await _apiService.CreateCategoryAsync(category);
                    await _apiService.LogHistoryAsync("Create", "Category", category.CategoryId, $"Đã tạo danh mục có tên: {category.Name}");
                    TempData["SuccessMessage"] = "Danh mục được tạo thành công.";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tạo danh mục. Vui lòng thử lại.";
                _logger.LogError(ex, "Error creating category");
            }

            return View(category);
        }


        // GET: Categories/Edit/5
        // [Authorize(Roles = "Write")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var category = await _apiService.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
      //  [Authorize(Roles = "Write")]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Name,Description")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient();
                AddAuthorizationHeader(client);
                if (category.Images == null)
                {
                    category.Images = new List<Image>();
                }

                await _apiService.UpdateCategoryAsync(id, category);
                await _apiService.LogHistoryAsync("Edit", "Category", category.CategoryId, $"Đã sửa danh mục có tên: {category.Name}");
                TempData["SuccessMessage"] = "Danh mục được cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        //[Authorize(Roles = "Write")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var category = await _apiService.GetCategoryByIdAsync(id.Value);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        //[Authorize(Roles = "Write")]

        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = _httpClientFactory.CreateClient();
            AddAuthorizationHeader(client);
            var category = await _apiService.GetCategoryByIdAsync(id);
            await _apiService.DeleteCategoryAsync(id);
            await _apiService.LogHistoryAsync("Delete", "Category", id, $"Đã xóa danh mục có tên: {category.Name}");
            TempData["SuccessMessage"] = "Đã xóa danh mục thành công.";
            return RedirectToAction(nameof(Index));
        }
    }
}
