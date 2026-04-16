using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Roxana_tema1.Middleware.Auth
{
    public class PermissionAuthorizationHandler : AuthorizationHandler<PrivilegeRequirement>
    {
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PrivilegeRequirement requirement)
        {
            var claimsPrincipals = context.User;
            if (!AreClaimsValid(claimsPrincipals))
            {
                context.Fail();
                await Task.CompletedTask;
                return;
            }

            var userClaimsModel = ExtractUserClaims(claimsPrincipals);
            if(userClaimsModel.ClaimRoles == null)
                context.Fail();
            else
                ValidateUserPrivileges(context, requirement, userClaimsModel.ClaimRoles);

            await Task.CompletedTask;
        }

        #region Private methods

        private void ValidateUserPrivileges(AuthorizationHandlerContext context, PrivilegeRequirement requirement, IEnumerable<string> claimRoles)
        {
            if (requirement.Role == Policies.All)
                if (claimRoles.Contains(Policies.User) || claimRoles.Contains(Policies.Admin))
                {
                    context.Succeed(requirement);
                    return;
                }
            if(claimRoles.Contains(requirement.Role))
                context.Succeed(requirement);
            else context.Fail();
        }

        private bool AreClaimsValid(ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal != null &&
                    claimsPrincipal.Identity != null &&
                    claimsPrincipal.Identity.IsAuthenticated &&
                    claimsPrincipal.HasClaim(c => c.Type.Equals(AuthorizationConstants.ClaimRole) &&
                    claimsPrincipal.HasClaim(c => c.Type.Equals(AuthorizationConstants.ClaimSubject)));
        }

        private UserClaimModel ExtractUserClaims(ClaimsPrincipal claimsPrincipal)
        {
            var claimRoleValues = claimsPrincipal
                .FindAll(c => c.Type.Equals(AuthorizationConstants.ClaimRole))
                .Select(c => c.Value);
            Guid.TryParse(claimsPrincipal
                .FindFirst(c => c.Type.Equals(AuthorizationConstants.ClaimSubject))?.Value, out Guid claimSubjectValue);
            return new UserClaimModel
            {
                ClaimUserId = claimSubjectValue,
                ClaimRoles = claimRoleValues
            };
        }
        #endregion
    }


}
