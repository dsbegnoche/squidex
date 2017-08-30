// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Squidex.Domain.Users;
using Squidex.Shared.Users;

namespace Squidex.Controllers.UI
{
    public static class Extensions
    {
        public static Task<IdentityResult> UpdateAsync(this UserManager<IUser> userManager, IUser user, string email, string displayName, string firstName, string lastName)
        {
            user.UpdateEmail(email);
            user.SetDisplayName(displayName);
            user.SetFirstName(firstName);
            user.SetLastName(lastName);

            return userManager.UpdateAsync(user);
        }

        public static async Task<ExternalLoginInfo> GetExternalLoginInfoWithDisplayNameAsync(this Domain.Users.Base.ISignInManager<IUser> signInManager, string expectedXsrf = null)
        {
            var externalLogin = await signInManager.GetExternalLoginInfoAsync(expectedXsrf);

            externalLogin.ProviderDisplayName = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

            return externalLogin;
        }
    }
}
