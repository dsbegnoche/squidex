// ==========================================================================
//  AppPatternUpdated.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppPatternUpdatedEvent")]
    public sealed class AppPatternUpdated : AppEvent
    {
        public string OriginalName { get; set; }

        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }
    }
}
