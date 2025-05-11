using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Entities
{
    public static class SecureKeysManager
    {
        public static Dictionary<string, string> SecureKeys { get; set; } = new Dictionary<string, string>()
        {
            { "jwtSecretKey", "NFL+X6v46RV6zS/AOqDgO9YtyFmutcwtxklNah7YRWw=" },
            { "appKey", "oG6kxiWrZoYHafEqvjCTd8apdQW/zTEmPGkpUIELwTs=" },
        };

        public static string GenerateJwtToken(string appKey)
        {
            // Validate the key value
            try
            {
                string expectedKey = SecureKeys?["appKey"];
                if (appKey != expectedKey)
                {
                    return null;
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecureKeys?["jwtSecretKey"].Trim()));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new[]
                {
                    new Claim(SecureKeys?["appKey"], appKey),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var token = new JwtSecurityToken(
                    issuer: "DbLocalizer",
                    audience: "DbLocalizer",
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials);

                string tokenValue = new JwtSecurityTokenHandler().WriteToken(token);
                if (ValidateJwtToken(tokenValue))
                {
                    return tokenValue;
                }
            }
            catch
            {
                return null;
            }

            return null;
        }

        private static bool ValidateJwtToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "DbLocalizer", // Replace with your actual issuer
                ValidateAudience = true,
                ValidAudience = "DbLocalizer", // Replace with your actual audience
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SecureKeys?["jwtSecretKey"].Trim())),
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                if (validatedToken != null && principal.Identity.IsAuthenticated)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
            return false;
        }
    }
}
