using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Roxana_tema1.Filters.Auth
{
    public class ClaimRequirementFilter : IAuthorizationFilter
    {
        readonly Claim _claim;

        public ClaimRequirementFilter(Claim claim)
        {
            _claim = claim;
        }   

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var hasClaim = context.HttpContext.User.Claims.Any(c => c.Type == _claim.Type && c.Value == _claim.Value);
            if (!hasClaim)
            {
                context.HttpContext.Response.StatusCode = 403;
            }
        }
    }
}