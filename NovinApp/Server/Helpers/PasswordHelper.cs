using System;
using System.Security.Cryptography;

namespace NovinApp.Server.Helpers
{
    public static class PasswordHelper
    {
        private const int SaltSize = 16; // 128-bit
        private const int KeySize = 32;  // 256-bit
        private const int Iterations = 100_000;
        private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;
        private const string Prefix = "$PBKDF2$";

        /// <summary>
        /// هش کردن امن کلمه عبور با استفاده از الگوریتم PBKDF2 و سالت تصادفی
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return string.Empty;

            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                Algorithm,
                KeySize
            );

            return $"{Prefix}{Iterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";
        }

        /// <summary>
        /// اعتبارسنجی کلمه عبور؛ پشتیبانی همزمان از کلمه‌های عبور هش‌شده جدید و کلمه‌های عبور قبلی (Plain-text) جهت جلوگیری از قفل شدن کاربران
        /// </summary>
        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedPassword))
                return false;

            // اگر به فرمت امن PBKDF2 ذخیره شده باشد
            if (IsHashed(storedPassword))
            {
                try
                {
                    var parts = storedPassword.Split('$');
                    // فرمت: "" / "PBKDF2" / iterations / salt / hash
                    if (parts.Length != 5) return false;

                    int iterations = int.Parse(parts[2]);
                    byte[] salt = Convert.FromBase64String(parts[3]);
                    byte[] expectedHash = Convert.FromBase64String(parts[4]);

                    byte[] actualHash = Rfc2898DeriveBytes.Pbkdf2(
                        inputPassword,
                        salt,
                        iterations,
                        Algorithm,
                        expectedHash.Length
                    );

                    return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
                }
                catch
                {
                    return false;
                }
            }

            // سازگاری با سیستم قبلی (Plain text) برای کاربرانی که از قبل در دیتابیس ثبت شده‌اند
            return string.Equals(storedPassword.Trim(), inputPassword.Trim(), StringComparison.Ordinal);
        }

        /// <summary>
        /// بررسی اینکه آیا کلمه عبور قبلاً هش شده است یا به صورت متن خام در دیتابیس قرار دارد
        /// </summary>
        public static bool IsHashed(string? password)
        {
            return !string.IsNullOrEmpty(password) && password.StartsWith(Prefix);
        }
    }
}
