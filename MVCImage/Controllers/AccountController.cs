using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using MVCImage.Models.DTO;

namespace MVCImage.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO model)
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7139/api/User/Login", model);

            if (response.IsSuccessStatusCode)
            {
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                var token = tokenResponse.JwtToken;
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();

                // Store roles in session
                HttpContext.Session.SetString("UserRoles", string.Join(",", roles));
                HttpContext.Session.SetString("AccessToken", token);
                HttpContext.Session.SetString("Username", model.Username);

                var claims = new List<Claim> { new Claim(ClaimTypes.Name, model.Username) };
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties { IsPersistent = true };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                return RedirectToAction("Index", "Categories");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Đăng nhập không thành công. Vui lòng thử lại.");
                return View(model);
            }
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var client = _httpClientFactory.CreateClient();
            var response = await client.PostAsJsonAsync("https://localhost:7139/api/User/Register", model);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công. Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Đăng ký không thành công. Vui lòng thử lại. Chi tiết lỗi: {errorContent}");
                return View(model);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đăng xuất thành công!";
            return RedirectToAction("Login");
        }
    }
}
     public class TokenResponse
    {
        public string JwtToken { get; set; }
    }