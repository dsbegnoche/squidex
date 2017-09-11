/*
 * CivicPlus implementation of Squidex Headless CMS
 */

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppPatternAddedEvent")]
    public sealed class AppPatternAdded : AppEvent
    {
        public string Name { get; set; }

        public string Pattern { get; set; }

        public string DefaultMessage { get; set; }
    }
}
