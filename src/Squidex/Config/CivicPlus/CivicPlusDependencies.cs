// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using CivicPlusIdentityServer.SDK.NetCore;
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

			services.AddSingleton<CivicPlusIdentityServer.SDK.NetCore.Base.IActions>(new Actions(options.CivicPlusIdentityServerBaseUrl));
			services.TryAddScoped<ISignInManager<IUser>, Squidex.Domain.Users.SignInManager<IUser>>();

			return services;
		}
	}
}
