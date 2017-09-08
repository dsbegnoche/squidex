using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Write;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Pipeline.CommandHandlers
{
    public sealed class EnrichWithRolesCommandMiddleware : ICommandMiddleware
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public EnrichWithRolesCommandMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is RolesCommand roleCommand)
            {
                var claims = httpContextAccessor.HttpContext.User.FindAll(JwtClaimTypes.Role);
                foreach (var claim in claims)
                {
                    roleCommand.Roles.Add(claim.Value);
                }
            }

            return next();
        }
    }
}
