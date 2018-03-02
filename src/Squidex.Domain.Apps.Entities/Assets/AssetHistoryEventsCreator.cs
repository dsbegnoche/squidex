// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.History;
using Squidex.Domain.Apps.Events.Assets;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public sealed class AssetHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public AssetHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<AssetCreated>(
                "created asset {[Name]}.");

            AddEventMessage<AssetUpdated>(
                "updated asset.");

            AddEventMessage<AssetDeleted>(
                "deleted asset.");

            AddEventMessage<AssetRenamed>(
                "renamed asset to {[Name]}.");
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            var channel = $"assets.{@event.Headers.AggregateId()}";

            var result = ForEvent(@event.Payload, channel);

            if (@event.Payload is AssetCreated createdEvent)
            {
                result = result.AddParameter("Name", createdEvent.FileName);
            }

            if (@event.Payload is AssetRenamed renamedEvent)
            {
                result = result.AddParameter("Name", renamedEvent.FileName);
            }

            return Task.FromResult(result);
        }
    }
}
