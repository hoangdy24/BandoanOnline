using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QlW_BandoanOnline.Hubs;
using QlW_BandoanOnline.Models;
using QlW_BandoanOnline.Repository;
using Microsoft.AspNetCore.StaticFiles;
using NETCore.MailKit.Core;
using NETCore.MailKit.Extensions;
using MailKit.Security;
using NETCore.MailKit.Infrastructure.Internal;
using QlW_BandoanOnline.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("vi-VN");
    options.SupportedCultures = new List<CultureInfo> { new CultureInfo("vi-VN") };
    options.SupportedUICultures = new List<CultureInfo> { new CultureInfo("vi-VN") };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<VnPayService>();

// Cấu hình session (nếu cần)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure DbContext
builder.Services.AddDbContext<DataContext>(options =>
{
    options.UseSqlServer(builder.Configuration["ConnectionStrings:ConnectedDB"]);
});
// SignalR configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
}).AddJsonProtocol(options => {
    options.PayloadSerializerOptions.PropertyNamingPolicy = null;
});

builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;
    options.User.RequireUniqueEmail = true;
});

builder.Services.AddRazorPages();

// Cấu hình MailKit
builder.Services.AddMailKit(config =>
{
    config.UseMailKit(new MailKitOptions
    {
        Server = builder.Configuration["EmailSettings:SmtpServer"],
        Port = int.Parse(builder.Configuration["EmailSettings:Port"]),
        SenderName = builder.Configuration["EmailSettings:FromName"],
        SenderEmail = builder.Configuration["EmailSettings:FromEmail"],
        Account = builder.Configuration["EmailSettings:Username"],
        Password = builder.Configuration["EmailSettings:Password"],
        Security = true
    });
});

// Đăng ký EmailService
builder.Services.AddScoped<QlW_BandoanOnline.Services.IEmailService, QlW_BandoanOnline.Services.EmailService>();

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roleNames = { "Admin", "NhanVien", "KhachHang", "QuanLy"};

        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                Console.WriteLine($"Đã tạo role {roleName} thành công!");
            }
        }

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var adminUser = await userManager.FindByEmailAsync("admin@qlw.com");

        if (adminUser == null)
        {
            var admin = new ApplicationUser
            {
                UserName = "admin@qlw.com",
                Email = "admin@qlw.com",
                HoTen = "Quản trị viên",
                EmailConfirmed = true,
            };

            var createAdmin = await userManager.CreateAsync(admin, "Admin@123");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                Console.WriteLine("Đã tạo tài khoản admin mặc định!");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Lỗi khi khởi tạo role: {ex.Message}");
    }
}

app.UseSession();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.MapHub<OrderHub>("/hubs/order");
app.MapHub<GioHangHub>("/GioHangHub");

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".webp"] = "image/webp";

app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "vnpay",
    pattern: "ThanhToan/VnPayCallback",
    defaults: new { controller = "ThanhToan", action = "VnPayCallback" });

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

var context = app.Services.CreateScope().ServiceProvider.GetRequiredService<DataContext>();
SeedData.SeedingData(context);

app.Run();
