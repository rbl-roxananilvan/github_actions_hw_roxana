using Microsoft.AspNetCore.Authorization;

namespace Roxana_tema1.Middleware.Auth
{
    public class PrivilegeRequirement : IAuthorizationRequirement
    {
        public string Role { get; private set; }

        public PrivilegeRequirement(string role)
        {
            Role = role;
        }
    }
}
