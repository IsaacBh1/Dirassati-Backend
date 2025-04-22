// Dirassati_Backend/Shared/Authorization/PermissionHandler.cs
using Microsoft.AspNetCore.Authorization;

namespace Dirassati_Backend.Features.Auth.Login
{
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            if (context.User.HasClaim(c => c.Type == "Permissions") &&
                int.TryParse(context.User.FindFirst("Permissions")?.Value, out int userPermissions) &&
                (userPermissions & requirement.RequiredPermission) == requirement.RequiredPermission)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }


    public class PermissionRequirement : IAuthorizationRequirement
    {
        public int RequiredPermission { get; }

        public PermissionRequirement(int requiredPermission)
        {
            RequiredPermission = requiredPermission;
        }
    }
}