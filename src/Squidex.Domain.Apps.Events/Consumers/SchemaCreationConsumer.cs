using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.CQRS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Events.Consumers
{
    public sealed class SchemaCreationConsumer : IEventConsumer
    {
        public SchemaCreationConsumer() { }

        // note: unsure what name means in consumer context currently.
        public string Name { get; } = "SchemaCreation";

        // match all schema events
        public string EventsFilter { get; } = "^schema-";

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        public async Task On(SchemaCreated addEvent, EnvelopeHeaders headers)
        {
            /*
            var stringField = new SchemaCreatedField()
            {
                Properties = new StringFieldProperties(),
                Name = "title",
                Partitioning = "Invariant",
                IsDisabled = false,
                IsHidden = false
            };

            addEvent.Fields.Add(stringField);

            this.DispatchAction(addEvent, headers);
            */
        }
    }
}
