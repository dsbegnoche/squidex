// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CivicPlusIdentityServer.SDK;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Squidex.Config.Identity;
using Squidex.Domain.Users.Base;
using Squidex.Shared.Users;

namespace Squidex.Config.CivicPlus
{
	public static class CivicPlusDependencies
	{
		public static IServiceCollection AddCivicPlusServices(this IServiceCollection services)
		{
			var serviceProvider = services.BuildServiceProvider();
			var options = serviceProvider.GetService<IOptions<MyIdentityOptions>>().Value;

			services.AddSingleton<CivicPlusIdentityServer.SDK.Base.IActions>(new Actions(options.CivicPlusIdentityServerBaseUrl));
			services.TryAddScoped<Squidex.Domain.Users.Base.ISignInManager<IUser>, Squidex.Domain.Users.SignInManager<IUser>>();

			return services;
		}
	}
}
