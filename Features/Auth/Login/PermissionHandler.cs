// Dirassati_Backend/Shared/Authorization/PermissionHandler.cs
using Microsoft.AspNetCore.Authorization;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (context.User.HasClaim(c => c.Type == "Permissions"))
        {
            var claimValue = context.User.FindFirst("Permissions")?.Value;
            if (int.TryParse(claimValue, out int userPermissions))
            {
                if ((userPermissions & requirement.RequiredPermission) == requirement.RequiredPermission)//note : this is the logic of autherization
                {
                    context.Succeed(requirement);
                }
            }
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
