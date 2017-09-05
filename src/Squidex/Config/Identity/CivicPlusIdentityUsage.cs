// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Microsoft.AspNetCore.Authentication.Cookies;
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
                    AuthenticationScheme = Constants.CivicPlusAuthenticationScheme,
                    SignInScheme = "Identity.External",
                    Authority = options.CivicPlusAuthority,
                    ClientId = options.CivicPlusClient,
                    ClientSecret = options.CivicPlusSecret,
                    DisplayName = "CivicPlus",
                    PostLogoutRedirectUri = options.CivicPlusPostLogoutRedirectUri,
                    ResponseType = options.CivicPlusResponseType,
                    Events = new CivicPlusHandler(),
                    SaveTokens = true
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
