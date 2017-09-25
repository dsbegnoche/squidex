// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Microsoft.AspNetCore.Builder;
using Squidex.Pipeline;

namespace Squidex.Config.CivicPlus
{
    public static class ClientThrottleUsages
    {
        public static IApplicationBuilder UseClientThrottle(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ClientIdHeaderMiddleware>();
        }
    }
}
