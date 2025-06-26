// Services/JwtService.cs
using Azure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PO_Api.Data;
using PO_Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class JwtService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;
    public JwtService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    public string GenerateAccessToken(User user)
    {
        var userRole = _db.Roles.FirstOrDefault(r => r.RoleId == user.RoleId).RoleName;
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.username),
            new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()),
            new Claim(ClaimTypes.Role, userRole)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"], // <<< ต้องมี
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    }

    public async Task SaveRefreshTokenAsync(User user, string token)
    {
        var refreshToken = new RefreshToken
        {
            Token = token,
            UserId = user.userId,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // ไม่ validate อายุ เพื่ออ่าน token ที่หมดอายุได้
            ValidIssuers = _config.GetSection("Jwt:Issuer").Get<string[]>(),
            ValidAudiences = _config.GetSection("Jwt:Audience").Get<string[]>(),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            if (validatedToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                return null;
            return principal;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.userId == userId);
        if (user == null)
            return false;

        var tokenInDb = await _db.RefreshTokens
            .FirstOrDefaultAsync(t => t.UserId == user.userId && t.Token == refreshToken && !t.IsRevoked);

        return tokenInDb != null && tokenInDb.ExpiresAt > DateTime.UtcNow;
    }


}