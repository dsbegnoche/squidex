// ==========================================================================
//  TableStorageEventStore.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using Streamstone;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class TableStorageEventStore : IEventStore
    {
        private const int AnyVersion = int.MinValue;
        private readonly CloudTable table;
        private readonly string prefix;

        public TableStorageEventStore(CloudTable table, string prefix)
        {
            this.prefix = prefix?.Trim(' ', '-').WithFallback("squidex");
            this.table = table;
        }

        public Task AppendEventsAsync(Guid commitId, string streamName, ICollection<EventData> events)
        {
            return AppendEventsAsync(commitId, streamName, -1, events);
        }

        public async Task AppendEventsAsync(Guid commitId, string streamName, int expectedVersion, ICollection<EventData> events)
        {
            Guard.GreaterEquals(expectedVersion, -1, nameof(expectedVersion));
            var i = expectedVersion;

            IEnumerable<TableEvent> tableEvents = events.Select(x => new TableEvent
            {
                Data = x
            });
            // iterate through current aggregate events increasing version with each processed event
            foreach (var @event in tableEvents)
            {
                i++;
                @event.EventPosition = i.ToString();
                @event.EventStreamNumber = i;
            }

            var partition = new Partition(table, GetStreamName(streamName));

            var existent = await Stream.TryOpenAsync(partition);
            var stream = existent.Found
                ? existent.Stream
                : new Stream(partition);

            if (stream.Version != expectedVersion)
            {
                throw new Exception();
            }

            try
            {
                await Stream.WriteAsync(stream, tableEvents.Select(ToEventData).ToArray());
            }
            catch (ConcurrencyConflictException e)
            {
                throw new Exception();
            }

            foreach (var @event in events)
            {
                // publish current event to the bus for further processing by subscribers
                //_publisher.Publish(@event);
            }
        }

        public IEventSubscription CreateSubscription()
        {
            throw new NotImplementedException();
        }

        public async Task<IReadOnlyList<StoredEvent>> GetEventsAsync(string streamName)
        {
            var partition = new Partition(table, GetStreamName(streamName));

            if (!(await Stream.ExistsAsync(partition)))
            {
                throw new KeyNotFoundException();
            }

            return (await Stream.ReadAsync<TableEvent>(partition)).Events.Select(x => x.ToStoredEvent()).ToList();
        }

        private static Streamstone.EventData ToEventData(TableEvent @event)
        {
            var id = Guid.NewGuid();
            var properties = new
            {
                Id = id,
                Type = @event.Data.Type,
                Data = JsonConvert.SerializeObject(@event)
            };
            return new Streamstone.EventData(EventId.From(id), EventProperties.From(properties));
        }

        private string GetStreamName(string streamName)
        {
            return $"{prefix}-{streamName}";
        }
    }
}
