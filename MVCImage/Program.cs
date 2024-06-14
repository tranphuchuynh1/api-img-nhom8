using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using MVCImage.Services;
using System.Net.Http.Headers;
using MVCImage.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký dịch vụ IHttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Đăng ký ApiService
builder.Services.AddTransient<ApiService>();

// Đăng ký dịch vụ HttpClient
builder.Services.AddHttpClient();

// Thêm xác thực và quyền hạn
var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
})
  .AddCookie(options =>
  {
      options.LoginPath = "/Account/Login";  // Đường dẫn khi xác thực thất bại
      options.AccessDeniedPath = "/Account/AccessDenied";  // Đường dẫn khi truy cập bị từ chối
      options.ExpireTimeSpan = TimeSpan.FromHours(1);  // Thời gian sống của Cookie
  });


// Add policies for roles
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("WritePolicy", policy => policy.RequireRole("Write"));
    options.AddPolicy("ReadPolicy", policy => policy.RequireRole("Read"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication(); // Đảm bảo bạn thêm dòng này để sử dụng xác thực
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Categories}/{action=Index}/{id?}");

app.Run();
