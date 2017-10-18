// ==========================================================================
//  ElasticExtensions.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Events;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Nest;
using Squidex.Domain.Apps.Read.Elastic;

namespace Squidex.Domain.Apps.Read.Elastic.Utils
{
    public static class ElasticExtensions
    {
        public static Task CreateAsync<T>(this IElasticClient client, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IElasticEntity, new()
        {
            var entity = EntityMapper.Create<T>(@event, headers);

            updater(entity);

            return client.InsertAsync()
        }

        public static async Task CreateAsync<T>(this IElasticClient collection, SquidexEvent @event, EnvelopeHeaders headers, Func<T, Task> updater) where T : class, IElasticEntity, new()
        {
            var entity = EntityMapper.Create<T>(@event, headers);

            await updater(entity);

            await collection.InsertOneIfNotExistsAsync(entity);
        }

        public static async Task UpdateAsync<T>(this IElasticClient collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IElasticEntity, new()
        {
            var entity =
                await collection.Find(t => t.Id == headers.AggregateId())
                    .FirstOrDefaultAsync();

            if (entity == null)
            {
                throw new DomainObjectNotFoundException(headers.AggregateId().ToString(), typeof(T));
            }

            await collection.UpdateAsync(@event, headers, entity, updater);
        }

        public static async Task<bool> TryUpdateAsync<T>(this IElasticClient collection, SquidexEvent @event, EnvelopeHeaders headers, Action<T> updater) where T : class, IElasticEntity, new()
        {
            var entity =
                await collection.Find(t => t.Id == headers.AggregateId())
                    .FirstOrDefaultAsync();

            if (entity != null)
            {
                if (entity is IEntityWithVersion withVersion)
                {
                    var eventVersion = headers.EventStreamNumber();

                    if (eventVersion <= withVersion.Version)
                    {
                        return false;
                    }
                }

                await collection.UpdateAsync(@event, headers, entity, updater);

                return true;
            }

            return false;
        }

        private static async Task UpdateAsync<T>(this IElasticClient collection, SquidexEvent @event, EnvelopeHeaders headers, T entity, Action<T> updater) where T : class, IElasticEntity, new()
        {
            EntityMapper.Update(@event, headers, entity);

            updater(entity);

            await collection.ReplaceOneAsync(t => t.Id == entity.Id, entity);
        }
    }
}
