// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Microsoft.AspNetCore.Authorization;
using Squidex.Shared.Identity;

namespace Squidex.Pipeline
{
    public sealed class MustBeAppAuthor : AuthorizeAttribute
    {
        public MustBeAppAuthor()
        {
            Roles = SquidexRoles.AppAuthor;
        }
    }
}
