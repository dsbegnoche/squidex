// ==========================================================================
//  ContentHistoryEventsCreator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.History;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Read.Contents
{
    public class ContentHistoryEventsCreator : HistoryEventsCreatorBase
    {
        public ContentHistoryEventsCreator(TypeNameRegistry typeNameRegistry)
            : base(typeNameRegistry)
        {
            AddEventMessage<ContentCreated>(
                "created content item.");

            AddEventMessage<ContentUpdated>(
                "updated content item.");

            AddEventMessage<ContentDeleted>(
                "deleted content item.");

            AddEventMessage<ContentStatusChanged>(
                "changed status of content item to {[Status]}.");
        }

        protected Task<HistoryEventToStore> On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return Task.FromResult(HandleEvent(@event, headers));
        }

        protected Task<HistoryEventToStore> On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return Task.FromResult(HandleEvent(@event, headers));
        }

        protected Task<HistoryEventToStore> On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return Task.FromResult(HandleEvent(@event, headers));
        }

        protected Task<HistoryEventToStore> On(ContentStatusChanged @event, EnvelopeHeaders headers)
        {
            return Task.FromResult(HandleEvent(@event, headers).AddParameter("Status", @event.Status));
        }

        protected override Task<HistoryEventToStore> CreateEventCoreAsync(Envelope<IEvent> @event)
        {
            return this.DispatchFuncAsync(@event.Payload, @event.Headers, (HistoryEventToStore)null);
        }

        private HistoryEventToStore HandleEvent(ContentEvent @event, EnvelopeHeaders headers)
        {
            return ForEvent(@event, $"contents.{headers.AggregateId()}");
        }
    }
}
