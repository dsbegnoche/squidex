using Microsoft.WindowsAzure.Storage.Table;

namespace Squidex.Infrastructure.CQRS.Events
{
    public class TableEvent : TableEntity
    {
        public string EventPosition { get; set; }

        public long EventStreamNumber { get; set; }

        public EventData Data { get; set; }

        public StoredEvent ToStoredEvent()
        {
            return new StoredEvent(EventPosition, EventStreamNumber, Data);
        }
    }
}
