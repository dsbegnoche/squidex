// ==========================================================================
//  ElasticRepository.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Threading.Tasks;
using Nest;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Tasks;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.Elastic.Utils;
using Squidex.Infrastructure.Reflection;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Read.Contents;
using Status = Squidex.Domain.Apps.Core.Contents.Status;

namespace Squidex.Domain.Apps.Read.Elastic
{
    public class ElasticRepository : IEventConsumer
    {
        private readonly string prefix;
        private readonly IElasticClient elasticClient;
        private readonly ISchemaProvider schemas;

        public ElasticRepository(string prefix, IElasticClient elasticClient, ISchemaProvider schemas)
        {
            this.prefix = prefix.ToLower();
            this.elasticClient = elasticClient;
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
            return elasticClient.CreateIndexAsync(new IndexName
            {
                Name = GetIndexName(@event.AppId.Id)
            });
        }

        protected Task On(AppDeleted @event, EnvelopeHeaders headers)
        {
            return elasticClient.DeleteIndexAsync(new IndexName
            {
                Name = GetIndexName(@event.AppId.Id)
            });
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (schema) =>
            {
                var content = EntityMapper.Create<ElasticContentEntity>(@event, headers);
                content.SchemaId = @event.SchemaId.Id;

                SimpleMapper.Map(@event, content);
                content.Status = Status.Draft;
                var idData = @event.Data?.ToIdModel(schema.SchemaDef, true);

                content.DataText = idData?.ToFullText();
                content.Data = idData.ToData(schema.SchemaDef, content.ReferencedIdsDeleted);
                content.ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef);
                return elasticClient.IndexAsync(content, i => i.Index(GetIndexName(@event.AppId.Id)));
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (schema) =>
            {
                var idData = @event.Data.ToIdModel(schema.SchemaDef, true);

                PartialElasticContentEntity content = new PartialElasticContentEntity
                {
                    DataText = idData?.ToFullText(),
                    ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef),
                    LastModified = headers.Timestamp(),
                    LastModifiedBy = @event.Actor,
                    Version = headers.EventStreamNumber(),
                };
                content.Data = idData.ToData(schema.SchemaDef, content.ReferencedIdsDeleted);

                return elasticClient.UpdateAsync<ElasticContentEntity, PartialElasticContentEntity>(new DocumentPath<ElasticContentEntity>(@event.ContentId), u => u
                    .Index(GetIndexName(@event.AppId.Id))
                    .Doc(content));
            });
        }

        protected Task On(ContentStatusChanged @event, EnvelopeHeaders headers)
        {
            return elasticClient.UpdateAsync<ElasticContentEntity, object>(
                new DocumentPath<ElasticContentEntity>(@event.ContentId), u => u
                    .Index($"{this.prefix}{@event.AppId.Id}")
                    .Doc(new { st = @event.Status }));
        }

        protected Task On(ContentDeleted @event, EnvelopeHeaders headers)
        {
            return elasticClient.DeleteAsync(
                new DocumentPath<ElasticContentEntity>(@event.ContentId), u => u
                    .Index(GetIndexName(@event.AppId.Id)));
        }

        private async Task ForSchemaAsync(Guid appId, Guid schemaId, Func<ISchemaEntity, Task> action)
        {
            var schema = await schemas.FindSchemaByIdAsync(schemaId, true);

            if (schema == null)
            {
                return;
            }

            await action(schema);
        }

        private string GetIndexName(Guid appId)
        {
            return $"{this.prefix}{appId}";
        }
    }
}
