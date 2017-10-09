// ==========================================================================
//  AppContributorAssigned.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Events.Apps.Old
{
    [EventType(nameof(AppContributorAssigned))]
    public sealed class AppContributorAssigned : AppEvent, IMigratedEvent
    {
        public string ContributorId { get; set; }

        public string Permission { get; set; }

        public IEvent Migrate()
        {
            AppContributorPermission permission;
            if (Permission == AppClientPermission.Reader.ToString())
            {
                permission = AppContributorPermission.Author;
            }
            else
            {
                System.Enum.TryParse(Permission, out permission);
            }

            return SimpleMapper.Map(this, new Apps.AppContributorAssigned { Permission = permission });
        }
    }
}
