using Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Roxana_tema1.Models;

namespace Roxana_tema1.Controllers
{
    [Route("api/v2/login")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "v2")]
    public class LoginController : ControllerBase
    {
        private readonly IUserService _userService;

        public LoginController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        [AllowAnonymous]
        [Produces("application/json")]
        public IActionResult Login([FromBody] AuthenticateRequest login)
        {
            IActionResult response = Unauthorized();
            var result = _userService.Authenticate(login.UserName, login.Password);

            if (result == null)
                return Unauthorized();

            return Ok(new AuthenticateResponse
            {
                IdToken = result.IdToken,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult Refresh([FromBody] RefreshTokenRequest refreshTokenRequest)
        {
            var result = _userService.RefreshTokens(refreshTokenRequest.RefreshToken);

            if (result == null)
                return Unauthorized();

            return Ok(new AuthenticateResponse
            {
                IdToken = result.IdToken,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }


        [HttpPost("renew-access")]
        [Authorize]
        [Produces("application/json")]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public IActionResult RefreshAccessToken()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized();
            }

            var accessToken = authHeader.Substring("Bearer ".Length).Trim();

            var result = _userService.RefreshAccessToken(accessToken);

            if (result == null)
                return Unauthorized();

            return Ok(new AuthenticateResponse
            {
                IdToken = result.IdToken,
                AccessToken = result.AccessToken,
                RefreshToken = result.RefreshToken
            });
        }
    }
}