using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticAssets;

namespace Dirassati_Backend.Common.Extensions;


public static class IdentityExtensions
{
    public static string ToCustomString(this IEnumerable<IdentityError> errors)
    {
        var result = "";
        foreach (var error in errors)
        {
            result += $"{error.Code}  ===> {error.Description}\n";
        }
        return result;
    }

}