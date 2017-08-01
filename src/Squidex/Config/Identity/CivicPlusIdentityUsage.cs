using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Squidex.Config.Identity
{
	public static class CivicPlusIdentityUsage
	{
		public static IApplicationBuilder UseMyCivicPlusPlatform(this IApplicationBuilder app)
		{
			var options = app.ApplicationServices.GetService<IOptions<MyIdentityOptions>>().Value;

			if (options.IsCivicPlusConfigured())
			{
				var civicPlusOptions = new OpenIdConnectOptions()
				{
					AuthenticationScheme = "CivicPlus",
					Authority = options.CivicPlusAuthority,
					ClientId = options.CivicPlusClient,
					ClientSecret = options.CivicPlusSecret,
					DisplayName = "CivicPlus",
					PostLogoutRedirectUri = options.CivicPlusPostLogoutRedirectUri,
					ResponseType = options.CivicPlusResponseType
				};

				foreach (var scope in options.CivicPlusScope.Split(' '))
				{
					civicPlusOptions.Scope.Add(scope);
				}

				app.UseOpenIdConnectAuthentication(civicPlusOptions);
			}

			return app;
		}
	}
}
