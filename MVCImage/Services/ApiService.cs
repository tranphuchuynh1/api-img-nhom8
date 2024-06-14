using System.Net.Http.Headers;
using System.Text;
using MVCImage.Models;
using Newtonsoft.Json;

namespace MVCImage.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri("https://localhost:7139/api/");
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }
        public List<string> GetUserRoles()
        {
            var roles = _httpContextAccessor.HttpContext.Session.GetString("UserRoles"); // Assuming roles are stored as "Read,Write"
            return roles?.Split(',').ToList() ?? new List<string>();
        }
        private void EnsureAuthorizationHeader()
        {
            if (_httpClient.DefaultRequestHeaders.Authorization == null)
            {
                var token = _httpContextAccessor.HttpContext.Session.GetString("AccessToken");
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _logger.LogError("Access Token is missing.");
                    throw new UnauthorizedAccessException("Authorization token is missing in the request headers.");
                }
            }
        }

        private bool UserHasRole(string role)
        {
            var roles = _httpContextAccessor.HttpContext.Session.GetString("UserRoles"); // Assuming roles are stored as "Read,Write"
            return roles?.Split(',').Contains(role) ?? false;
        }

        private void CheckRoleForWrite()
        {
            if (!UserHasRole("Write") && !UserHasRole("Read"))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền để thực hiện hành động này.");
            }
        }

        public async Task LogHistoryAsync(string action, string entity, int entityId, string details = "")
        {
            var history = new History
            {
                Action = action,
                Entity = entity,
                EntityId = entityId,
                Details = details, // Thêm thông tin chi tiết vào lịch sử
                Timestamp = DateTime.UtcNow
            };

            var json = JsonConvert.SerializeObject(history);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("histories", content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to log history for action {Action} on entity {Entity} with id {EntityId}. Status Code: {StatusCode}", action, entity, entityId, response.StatusCode);
            }
        }

        public async Task<IEnumerable<History>> GetHistoriesAsync()
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.GetAsync("histories");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<History>>(content);
        }

        public async Task ClearHistoriesAsync()
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.DeleteAsync($"Histories/clear");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error clearing histories: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.GetAsync("Categories");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Category>>(content);
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Categories/{id}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Category>(content);
        }

        public async Task CreateCategoryAsync(Category category)
        {
            EnsureAuthorizationHeader();
            CheckRoleForWrite();
            if (category.Images == null)
            {
                category.Images = new List<Image>();
            }
            var response = await _httpClient.PostAsJsonAsync("Categories", category);
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden || response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    throw new UnauthorizedAccessException("Bạn không có quyền dùng chức năng này.");
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error creating category: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task UpdateCategoryAsync(int id, Category category)
        {
            EnsureAuthorizationHeader();
            CheckRoleForWrite();
            var json = JsonConvert.SerializeObject(category);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Categories/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error updating category: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            EnsureAuthorizationHeader();
            CheckRoleForWrite();
            var response = await _httpClient.DeleteAsync($"Categories/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error deleting category: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task<IEnumerable<Image>> GetImagesAsync()
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.GetAsync("Images");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Image>>(content);
        }

        public async Task<Image> GetImageByIdAsync(int id)
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Images/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to retrieve image with id {Id}. Status Code: {StatusCode}", id, response.StatusCode);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<Image>(content);
        }

        public async Task CreateImageAsync(ImageUploadDto imageDto)
        {
            EnsureAuthorizationHeader();
            CheckRoleForWrite();
            var json = JsonConvert.SerializeObject(imageDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Images", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error creating image: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task UpdateImageAsync(int id, Image image)
        {
            EnsureAuthorizationHeader();
            CheckRoleForWrite();
            var json = JsonConvert.SerializeObject(image);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"Images/{id}", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error updating image: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task DeleteImageAsync(int id)
        {
            EnsureAuthorizationHeader();
            CheckRoleForWrite();
            var response = await _httpClient.DeleteAsync($"Images/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error deleting image: {response.StatusCode}, {errorContent}");
            }
        }

        public async Task<IEnumerable<Image>> SearchImagesAsync(string title, string description)
        {
            EnsureAuthorizationHeader();
            var response = await _httpClient.GetAsync($"Images/Search?title={title}&description={description}");
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<IEnumerable<Image>>(content);
        }
    }
}