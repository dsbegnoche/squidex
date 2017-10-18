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
using Status = Squidex.Domain.Apps.Core.Contents.Status;
using Squidex.Domain.Apps.Read.Contents;

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
            return elasticClient.CreateIndexAsync(new IndexName()
            {
                Name = $"{this.prefix}{@event.AppId.Id}"
            });
        }

        protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (schema) =>
            {
                return elasticClient.CreateAsync<ElasticContentEntity>($"{this.prefix}{@event.AppId.Id}", @event, headers, content =>
                {
                    content.SchemaId = @event.SchemaId.Id;

                    SimpleMapper.Map(@event, content);
                    content.Status = Status.Draft;

                    var idData = @event.Data?.ToIdModel(schema.SchemaDef, true);

                    content.DataText = idData?.ToFullText();
                    content.Data = idData.ToData(schema.SchemaDef, content.ReferencedIdsDeleted);
                    content.ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef);
                });
            });
        }

        protected Task On(ContentUpdated @event, EnvelopeHeaders headers)
        {
            return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (schema) =>
            {
                return elasticClient.UpdateAsync<ElasticContentEntity>($"{this.prefix}{@event.AppId.Id}", @event, headers,
                    content =>
                    {
                        content.SchemaId = @event.SchemaId.Id;

                        SimpleMapper.Map(@event, content);

                        var idData = @event.Data.ToIdModel(schema.SchemaDef, true);

                        content.DataText = idData?.ToFullText();
                        content.Data = idData.ToData(schema.SchemaDef, content.ReferencedIdsDeleted);
                        content.ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef);
                        content.LastModified = headers.Timestamp();
                        content.LastModifiedBy = @event.Actor;
                        content.Version = headers.EventStreamNumber();
                    });
            });
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
    }
}
