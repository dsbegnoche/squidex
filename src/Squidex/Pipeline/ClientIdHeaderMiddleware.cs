// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Config;

namespace Squidex.Pipeline
{
    public class ClientIdHeaderMiddleware
    {
        private readonly RequestDelegate next;

        public ClientIdHeaderMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        /// <summary>
        /// Adds the Squidex frontend client ID header to the request if call is not coming from an API.
        /// </summary>
        /// <param name="context">HttpContext</param>
        public async Task Invoke(HttpContext context)
        {
            if (context != null && context.User.IsFrontendClient())
            {
                IHeaderDictionary headers = context.Request.Headers;
                headers["X-ClientId"] = Constants.FrontendClient;
            }

            await next(context);
        }
    }
}
