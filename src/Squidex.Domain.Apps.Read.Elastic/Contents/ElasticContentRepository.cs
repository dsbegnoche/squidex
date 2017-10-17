// ==========================================================================
//  ElasticContentRepository.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents
{
    public class ElasticContentRepository : IEventConsumer
    {
        private readonly ISchemaProvider schemas;

        public ElasticContentRepository(string index, ISchemaProvider schemas)
        {
            Guard.NotNull(index, nameof(index));
            Guard.NotNull(schemas, nameof(schemas));

            this.schemas = schemas;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(content-)|(app-)|(asset-)"; }
        }

        public Task ClearAsync()
        {
            //Delete all content indexes to be rebuilt.
            throw new NotImplementedException();
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }
    }
}
