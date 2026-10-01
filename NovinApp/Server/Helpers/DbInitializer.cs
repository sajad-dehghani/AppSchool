using Microsoft.EntityFrameworkCore;
using NovinApp.Server.MyContext;
using NovinApp.Shared;
using NovinApp.Shared.Constants;

namespace NovinApp.Server.Helpers
{
    public static class DbInitializer
    {
        public static async Task SeedDefaultAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<MyAppContext>();
            var logger = scope.ServiceProvider.GetService<ILogger<Program>>();

            try
            {
                // بررسی وجود هرگونه کاربر با نقش مدیر سیستم یا کد ملی admin
                var adminUser = await context.User.FirstOrDefaultAsync(u =>
                    u.code_meli == "admin" ||
                    u.Rool == UserRoles.SystemAdmin ||
                    u.Rool == "مدیر سیستم");

                if (adminUser == null)
                {
                    var defaultAdmin = new User
                    {
                        fname = "مدیر کل سیستم",
                        code_meli = "admin",
                        gender = "مرد",
                        mobile = "09120000000",
                        pass = PasswordHelper.HashPassword("Admin@123456"),
                        Rool = UserRoles.SystemAdmin,
                        active = true,
                        Id_School = null,
                        Id_Moshaver = null,
                        Moshaver = null
                    };

                    await context.User.AddAsync(defaultAdmin);
                    await context.SaveChangesAsync();

                    logger?.LogInformation("کاربر پیش‌فرض مدیر سیستم (SystemAdmin) با نام کاربری 'admin' با موفقیت ایجاد شد.");
                }
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "خطا در ایجاد کاربر پیش‌فرض مدیر سیستم");
            }
        }
    }
}
