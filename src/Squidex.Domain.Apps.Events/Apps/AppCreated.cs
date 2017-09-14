// ==========================================================================
//  AppCreated.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Events.Apps
{
    [EventType(nameof(AppCreated))]
    public sealed class AppCreated : AppEvent
    {
        public string Name { get; set; }
    }
}
