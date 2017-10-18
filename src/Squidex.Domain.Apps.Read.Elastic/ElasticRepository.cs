using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nest;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Elastic
{
    public class ElasticRepository : IEventConsumer
    {
        private readonly string prefix;
        private readonly IElasticClient elasticClient;

        public ElasticRepository(string prefix, IElasticClient elasticClient)
        {
            this.prefix = prefix.ToLower();
            this.elasticClient = elasticClient;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public string EventsFilter
        {
            get { return "^(content-)|(app-)|(asset-)"; }
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        public Task ClearAsync()
        {
            Indices.ManyIndices deleteIndices = Indices.Index($"{prefix}*");

            var response = elasticClient.DeleteIndex(new DeleteIndexRequest(deleteIndices));

            return TaskHelper.Done;
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
