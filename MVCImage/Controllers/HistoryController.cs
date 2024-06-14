using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVCImage.Services;
using System.Threading.Tasks;

namespace MVCImage.Controllers
{

    public class HistoryController : Controller
    {

        private readonly ApiService _apiService;
        public HistoryController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var histories = await _apiService.GetHistoriesAsync();
            // Lấy tên người dùng từ session và truyền ra view
            var username = HttpContext.Session.GetString("Username");
            ViewBag.Username = username;
            return View(histories);
        }

        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            await _apiService.ClearHistoriesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
