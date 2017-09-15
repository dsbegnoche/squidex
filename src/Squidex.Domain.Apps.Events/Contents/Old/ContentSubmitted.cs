// ==========================================================================
//  ContentSubmitted.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Events.Contents.Old
{
    [EventType(nameof(ContentSubmitted))]
    [Obsolete]
    public sealed class ContentSubmitted : ContentEvent, IMigratedEvent
    {
        public IEvent Migrate()
        {
            return SimpleMapper.Map(this, new ContentStatusChanged { Status = Status.Submitted });
        }
    }
}
