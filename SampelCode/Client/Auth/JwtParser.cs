using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;

namespace NovinApp.Client.Auth
{
    public class JwtParser
    {
        public static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();

            if (string.IsNullOrWhiteSpace(jwt))
                return claims;

            var parts = jwt.Split('.');
            if (parts.Length < 2)
                return claims;

            try
            {
                var payload = parts[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);

                using var doc = JsonDocument.Parse(jsonBytes);
                var root = doc.RootElement;

                string? nameValue = null;
                string? idValue = null;
                var roles = new List<string>();

                foreach (var prop in root.EnumerateObject())
                {
                    var key = prop.Name;

                    // رد کردن کلیم‌های زمانی
                    if (key.Equals("exp", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("nbf", StringComparison.OrdinalIgnoreCase) ||
                        key.Equals("iat", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in prop.Value.EnumerateArray())
                        {
                            var strVal = item.ToString();
                            claims.Add(new Claim(key, strVal));

                            if (key.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                                key.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
                                key.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
                            {
                                roles.Add(strVal);
                            }
                        }
                    }
                    else
                    {
                        var strVal = prop.Value.ToString();
                        claims.Add(new Claim(key, strVal));

                        if (key.Equals("unique_name", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals("name", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals(ClaimTypes.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            nameValue ??= strVal;
                        }

                        if (key.Equals("nameid", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals("UserId", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals("sub", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals(ClaimTypes.NameIdentifier, StringComparison.OrdinalIgnoreCase))
                        {
                            idValue ??= strVal;
                        }

                        if (key.Equals("role", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals("roles", StringComparison.OrdinalIgnoreCase) ||
                            key.Equals(ClaimTypes.Role, StringComparison.OrdinalIgnoreCase))
                        {
                            roles.Add(strVal);
                        }
                    }
                }

                // اطمینان از وجود ClaimTypes.Name برای Identity.Name
                if (!claims.Any(c => c.Type == ClaimTypes.Name) && !string.IsNullOrEmpty(nameValue))
                {
                    claims.Add(new Claim(ClaimTypes.Name, nameValue));
                }

                // اطمینان از وجود ClaimTypes.NameIdentifier
                if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && !string.IsNullOrEmpty(idValue))
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, idValue));
                }

                // اطمینان از وجود ClaimTypes.Role برای AuthorizeView و IsInRole
                foreach (var role in roles)
                {
                    if (!claims.Any(c => c.Type == ClaimTypes.Role && c.Value == role))
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JWT claims: {ex.Message}");
            }

            return claims;
        }

        private static byte[] ParseBase64WithoutPadding(string base64)
        {
            // جایگزینی کاراکترهای Base64Url
            base64 = base64.Replace('-', '+').Replace('_', '/');

            // اضافه کردن پدینگ استاندارد
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }

            return Convert.FromBase64String(base64);
        }
    }
}
