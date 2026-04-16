using System;
using System.Collections.Generic;
using System.Text;
using Common.DTOs;
using Common.Entities;

namespace Interfaces
{
    public interface IUserService
    {
        AuthenticateResponseDto? Authenticate(string username, string password);
        IEnumerable<User> GetAll();
        User? GetById(Guid id);
        AuthenticateResponseDto? RefreshTokens(string refreshToken);
        AuthenticateResponseDto? RefreshAccessToken(string accessToken);
        User? GetUserByEmail(string email);
    }
}
