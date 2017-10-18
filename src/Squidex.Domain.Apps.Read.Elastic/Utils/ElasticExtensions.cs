// ==========================================================================
//  ElasticExtensions.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure.CQRS.Events;
using Nest;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Apps.Read.Elastic.Utils
{
    public static class ElasticExtensions
    {
        public static Task CreateAsync<T>(this IElasticClient client, string indexName, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IElasticEntity, new()
        {
            var entity = EntityMapper.Create<T>(@event, headers);

            updater(entity);

            return client.IndexAsync(entity, i => i.Index(new IndexName()
            {
                Name = indexName
            }));
        }

        public static Task UpdateAsync<T>(this IElasticClient client, string indexName, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IElasticEntity, new()
        {
            //When I left this was causing a duplicate to be placed in the index.
            //var entity = EntityMapper.Create<T>(@event, headers);

            //updater(entity);

            //var updateRequest = new UpdateRequest<IElasticEntity, IElasticEntity>(entity,
            //    new IndexName()
            //    {
            //        Name = indexName
            //    }
            //);

            //updateRequest.DocAsUpsert = true;
            //updateRequest.Doc = entity;
            //updateRequest.Upsert = entity;
            //updateRequest.RetryOnConflict = 3;
            //updateRequest.Refresh = Refresh.True;

            //var response = client.Update<IElasticEntity, IElasticEntity>(updateRequest);

            return TaskHelper.Done;
        }
    }
}
