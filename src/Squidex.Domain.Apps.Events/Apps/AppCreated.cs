// ==========================================================================
//  AppCreated.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppCreatedEvent")]
    public sealed class AppCreated : AppEvent
    {
        public string Name { get; set; }
    }
}
