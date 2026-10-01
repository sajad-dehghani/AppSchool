using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using NovinApp.Server.Middleware;
using NovinApp.Server.Models;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using NovinApp.Shared;

namespace NovinApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ۱. ثبت سرویس‌های پایه‌ای MVC و Razor
            builder.Services.AddControllersWithViews();
            builder.Services.AddRazorPages();
            builder.Services.AddMudServices();
            builder.Services.AddHttpClient();
            builder.Services.AddHttpContextAccessor();

            // ۲. پشتیبانی انکودینگ برای زبان فارسی و فایل‌های اکسل
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // ۳. تنظیم محدودیت حجم آپلود فایل (حداکثر ۱۰۰ مگابایت استاندارد)
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 104_857_600; // 100 MB
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 104_857_600; // 100 MB
            });

            // ۴. پیکربندی پایگاه‌داده با EF Core
            var connectionString = builder.Configuration.GetConnectionString("con")
                ?? "Data Source=.;Initial Catalog=NovinApp;Integrated Security=true;TrustServerCertificate=True";

            builder.Services.AddDbContext<MyAppContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            // ۵. پیکربندی ASP.NET Core Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // سیاست‌های کلمه عبور
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // سیاست‌های قفل حساب در ورود ناموفق
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // تنظیمات کاربر
                options.User.RequireUniqueEmail = false;
            })
            .AddEntityFrameworkStores<MyAppContext>()
            .AddDefaultTokenProviders();

            // ۶. پیکربندی احراز هویت با JWT
            var jwtKey = builder.Configuration["jwt:Key"] ?? "NovinAppProductionSecretKeyWithSufficientLength2026!#$";
            var jwtIssuer = builder.Configuration["jwt:Issuer"] ?? "NovinAppServer";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtIssuer,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            // ۷. ثبت سرویس‌های اختصاصی معماری تمیز
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddScoped<IExcelImportService, ExcelImportService>();
            builder.Services.AddScoped<IExamReportService, ExamReportService>();
            builder.Services.AddScoped<IDbInitializer, DbInitializer>();

            // ۸. سرویس‌های درگاه پرداخت (با مدیریت عدم وجود کانفیگ)
            var zarinPalOptions = builder.Configuration.GetSection("ZarinPal").Get<ZarinPalOptions>()
                ?? new ZarinPalOptions { MerchantId = "00000000-0000-0000-0000-000000000000", IsDevelopment = true };
            builder.Services.AddSingleton(zarinPalOptions);
            builder.Services.AddHttpClient<ZarinpalService>();
            builder.Services.AddScoped<ZarinpalService>();

            // ۹. ساخت و پیکربندی خط پردازش (Pipeline)
            var app = builder.Build();

            // اجرای DbInitializer جهت ایجاد دیتابیس و داده‌های اولیه در هنگام شروع برنامه
            using (var scope = app.Services.CreateScope())
            {
                try
                {
                    var initializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
                    await initializer.InitializeAsync();
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "خطا در مقداردهی اولیه پایگاه داده و Seed Data.");
                }
            }

            // میدلور مدیریت سراسری خطاها
            app.UseMiddleware<GlobalExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToFile("index.html");

            await app.RunAsync();
        }
    }
}
