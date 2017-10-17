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
using Squidex.Domain.Apps.Events.Contents;

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
            // Delete all content indexes to be rebuilt.
            throw new NotImplementedException();
        }

        public Task On(Envelope<IEvent> @event)
        {
            return this.DispatchActionAsync(@event.Payload, @event.Headers);
        }

        // On(AppCreated) => CreateIndex?

        //protected Task On(ContentCreated @event, EnvelopeHeaders headers)
        //{
        //    return ForSchemaAsync(@event.AppId.Id, @event.SchemaId.Id, (collection, schema) =>
        //    {
        //        return collection.CreateAsync(@event, headers, content =>
        //        {
        //            content.SchemaId = @event.SchemaId.Id;

        //            SimpleMapper.Map(@event, content);
        //            content.Status = Status.Draft;

        //            var idData = @event.Data?.ToIdModel(schema.SchemaDef, true);

        //            content.DataText = idData?.ToFullText();
        //            content.DataDocument = idData?.ToBsonDocument();
        //            content.ReferencedIds = idData?.ToReferencedIds(schema.SchemaDef);
        //        });
        //    });
        //}
    }
}
