using System.Text;
using Microsoft.AspNetCore.Identity;

namespace Dirassati_Backend.Common.Extensions;


public static class IdentityExtensions
{
    public static string ToCustomString(this IEnumerable<IdentityError> errors)
    {
        var result = new StringBuilder();
        foreach (var error in errors)
        {
            result.AppendLine($"{error.Code}  ===> {error.Description}");
        }
        return result.ToString();
    }

}