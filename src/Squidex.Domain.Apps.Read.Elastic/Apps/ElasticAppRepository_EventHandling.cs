using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;

namespace Squidex.Domain.Apps.Read.Elastic.Apps
{
    public class ElasticAppRepository_EventHandling : IEventConsumer
    {
        private readonly string prefix;
        private readonly IElasticClient elasticClient;

        public ElasticAppRepository_EventHandling(string prefix, IElasticClient elasticClient)
        {
            this.prefix = prefix;
            this.elasticClient = elasticClient;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^app-"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        public Task ClearAsync()
        {
            // Delete Index
            throw new NotImplementedException();
        }

        protected Task On(AppCreated @event, EnvelopeHeaders headers)
        {
            return elasticClient.CreateIndexAsync(new IndexName()
            {
                Name = this.prefix + @event.AppId.Id
            });
        }
    }
}
