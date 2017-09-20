// ==========================================================================
//  AppPatternDeleted.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppPatternDeletedEvent")]
    public sealed class AppPatternDeleted : AppEvent
    {
        public string Name { get; set; }
    }
}
