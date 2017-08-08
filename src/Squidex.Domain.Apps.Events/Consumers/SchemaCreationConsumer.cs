using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.CQRS.Events
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

        public async Task On(Envelope<IEvent> @event)
        {
            // this is hit currently.
            Console.WriteLine("hit");
        }
    }
}
