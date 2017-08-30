// ==========================================================================
//  AppDeleted.cs
//  Squidex Headless CMS
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Apps
{
    [TypeName("AppDeletedEvent")]
    public class AppDeleted : AppEvent
    {
        public string Name { get; set; }
    }
}
