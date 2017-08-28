// ==========================================================================
//  AppCreated.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
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
