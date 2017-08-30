// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;

namespace Squidex.Domain.Apps.Write
{
    public abstract class RolesCommand : SchemaCommand
    {
        protected RolesCommand()
        {
            Roles = new List<string>();
        }

        public List<string> Roles { get; set; }
    }
}
