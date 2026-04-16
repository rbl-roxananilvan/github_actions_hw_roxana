using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Text;
using Common;
using Common.DTOs;
using Common.Entities;
using Interfaces;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Services
{
    internal class RefreshToken
    {
        public string Token { get; set; } = null!;
        public Guid UserId { get; set; }
        public DateTime Expiry { get; set; }
    }

    public class UserService : IUserService
    {
        private List<User> _users = new List<User>();
        private readonly AuthorizationSettings _authorizationSettings;
        private readonly byte[] _salt;
        private readonly ConcurrentDictionary<string, RefreshToken> _refreshToken = new();


        public UserService(IOptions<AuthorizationSettings> appSetings)
        {
            _authorizationSettings = appSetings.Value;
            _salt = new byte[128 / 8];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create()) 
            {
                rng.GetBytes(_salt);
            }

            _users.Add(new User { Id = Guid.NewGuid(), FirstName = "Test", LastName = "User", Username = "test", Email = "user@gmail.com", PasswordHash = HashPassword("test"), UserRole = "User" });
            _users.Add(new User { Id = Guid.NewGuid(), FirstName = "Test2", LastName = "User2", Username = "test-admin", Email = "admin@gmail.com", PasswordHash = HashPassword("admin"), UserRole = "Admin" });
        }
        public AuthenticateResponseDto? Authenticate(string username, string password)
        {
            var user = _users.SingleOrDefault(x => x.Username == username && x.PasswordHash == HashPassword(password));
            if (user == null) return null;

            return new AuthenticateResponseDto
            {
                IdToken = GenerateIdToken(user),
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public AuthenticateResponseDto? RefreshTokens(string refreshToken)
        {
            if(!_refreshToken.TryGetValue(refreshToken, out var tokenEntry))
                return null;

            if(tokenEntry.Expiry < DateTime.UtcNow)
            {
                _refreshToken.TryRemove(refreshToken, out _);
                return null;
            }

            var user = _users.FirstOrDefault(x => x.Id == tokenEntry.UserId);

            if (user == null) return null;

            _refreshToken.TryRemove(refreshToken, out _);

            return new AuthenticateResponseDto
            {
                IdToken = GenerateIdToken(user),
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public AuthenticateResponseDto? RefreshAccessToken(string accessToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
            if (userIdClaim == null) return null;
            if (!Guid.TryParse(userIdClaim, out Guid userId)) return null;
            var user = _users.FirstOrDefault(x => x.Id == userId);
            if (user == null) return null;

            CleanupExpiredRefreshTokens();

            return new AuthenticateResponseDto
            {
                IdToken = GenerateIdToken(user),
                AccessToken = GenerateAccessToken(user),
                RefreshToken = GenerateRefreshToken(user)
            };
        }

        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        public User? GetUserByEmail(string email)
        {
            return _users.FirstOrDefault(x => x.Email == email);
        }

        public User? GetById(Guid id)
        {
            return _users.FirstOrDefault(x => x.Id == id);
        }

        private string GenerateIdToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authorizationSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("tokenType", "idToken"),
                new Claim("intership_year", "2026")
            };

            var token = new JwtSecurityToken(
                issuer: _authorizationSettings.Issuer,
                audience: _authorizationSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_authorizationSettings.IdTokenLifetimeInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private string GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authorizationSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("id", user.Id.ToString()),
                new Claim("role", user.UserRole),
                new Claim("tokenType", "access_token")
            };

            var token = new JwtSecurityToken(
                issuer: _authorizationSettings.Issuer,
                audience: _authorizationSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_authorizationSettings.AccessTokenLifetimeInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);

        }

        private string GenerateRefreshToken(User user)
        {
           var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var entry = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                Expiry = DateTime.UtcNow.AddMinutes(_authorizationSettings.RefreshTokenLifetimeInMinutes)
            };

            _refreshToken[refreshToken] = entry;

            CleanupExpiredRefreshTokens();

            return refreshToken;
        }

        private void CleanupExpiredRefreshTokens()
        {
            var expiredTokens = _refreshToken
                .Where(x => x.Value.Expiry < DateTime.UtcNow)
                .Select(x => x.Key)
                .ToList();

            foreach (var token in expiredTokens)
            {
                _refreshToken.TryRemove(token, out _);
            }
        }

        private string GenerateJWTToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authorizationSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName),
            new Claim("role",user.UserRole),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: _authorizationSettings.Issuer,
                audience: _authorizationSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_authorizationSettings.TokenLifetimeInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string HashPassword(string password)
        {
            return Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: _salt,
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8));
        }
    }
}
