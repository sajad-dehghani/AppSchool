using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor.Services;
using NovinApp.Server.Helpers;
using NovinApp.Server.MyContext;
using NovinApp.Server.Services;
using System.Text;

namespace NovinApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddMudServices();
            builder.Services.AddRazorPages();
            builder.Services.AddHttpClient();

            // پشتیبانی انکودینگ فارسی
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

            // تنظیم حداکثر حجم فایل
            builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
            {
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = 524_288_000;
                options.MultipartHeadersLengthLimit = int.MaxValue;
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 524_288_000;
            });

            // DbContext - ساده بدون Identity
            builder.Services.AddDbContext<MyAppContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("con"));
            });

            // JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(option =>
                {
                    var jwtKey = builder.Configuration["jwt:Key"] ?? "NovinAppSecretKeyMustBeAtLeast32CharsLong12345!";
                    option.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            builder.Services.AddAuthorization();

            // Services
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
            builder.Services.AddScoped<IAuditService, AuditService>();

            // فشرده‌سازی پاسخ‌ها (Brotli و Gzip) برای کاهش چشمگیر حجم دانلود اولیه بلیزور
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<BrotliCompressionProvider>();
                options.Providers.Add<GzipCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
                {
                    "application/octet-stream",
                    "application/wasm",
                    "font/woff2",
                    "font/woff",
                    "image/svg+xml",
                    "text/javascript",
                    "application/javascript"
                });
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Fastest;
            });
            builder.Services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Fastest;
            });

            var app = builder.Build();

            // فعال‌سازی فشرده‌سازی در پایپ‌لاین قبل از فایل‌های استاتیک
            app.UseResponseCompression();

            // ساخت کاربر پیش‌فرض مدیر سیستم در صورت عدم وجود
            await DbInitializer.SeedDefaultAdminAsync(app.Services);

            if (app.Environment.IsDevelopment())
                app.UseWebAssemblyDebugging();
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();

            // تنظیم کش مرورگر برای بارگذاری فوق‌العاده سریع در دفعات بعدی
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    var path = ctx.Context.Request.Path.Value ?? string.Empty;
                    if (path.StartsWith("/_framework/") || path.Contains(".wasm") || path.Contains(".woff2"))
                    {
                        // کش دائمی برای فایل‌های کامپایل‌شده و فونت‌ها
                        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
                    }
                    else if (path.EndsWith("index.html") || string.IsNullOrEmpty(path) || path == "/")
                    {
                        // عدم کش index.html جهت دریافت همیشگی آخرین نسخه
                        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
                    }
                    else
                    {
                        // کش متعادل برای سایر استایل‌ها و تصاویر (30 روز)
                        ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=2592000");
                    }
                }
            });

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
