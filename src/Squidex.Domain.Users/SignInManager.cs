// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public class SignInManager<TUser>: Microsoft.AspNetCore.Identity.SignInManager<IUser>, Base.ISignInManager<IUser>
    {
	    public SignInManager(UserManager<IUser> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<IUser> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<IUser>> logger)
			: base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger)
	    {
	    }

		public async Task<ExternalLoginInfo> GetExternalLoginInfoWithDisplayNameAsync(string expectedXsrf = null)
		{
			var externalLogin = await GetExternalLoginInfoAsync(expectedXsrf);

			externalLogin.ProviderDisplayName = externalLogin.Principal.FindFirst(ClaimTypes.Email).Value;

			return externalLogin;
		}
	}
}
