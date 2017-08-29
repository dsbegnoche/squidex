// ==========================================================================
//  ContentCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

// ReSharper disable ConvertToLambdaExpression

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAppProvider appProvider;
        private readonly IAssetRepository assetRepository;
        private readonly IContentRepository contentRepository;
        private readonly ISchemaProvider schemas;

        public ContentCommandMiddleware(
            IAggregateHandler handler,
            IAppProvider appProvider,
            IAssetRepository assetRepository,
            ISchemaProvider schemas,
            IContentRepository contentRepository)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(schemas, nameof(schemas));
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(appProvider, nameof(appProvider));
            Guard.NotNull(assetRepository, nameof(assetRepository));
            Guard.NotNull(contentRepository, nameof(contentRepository));

            this.handler = handler;
            this.schemas = schemas;
            this.appProvider = appProvider;
            this.assetRepository = assetRepository;
            this.contentRepository = contentRepository;
        }

        protected async Task On(CreateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to create content", true);

            await handler.CreateAsync<ContentDomainObject>(context, c =>
            {
                c.Create(command);

                context.Complete(EntityCreatedResult.Create(command.Data, c.Version));
            });
        }

        protected async Task On(UpdateContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to update content");

            await handler.UpdateAsync<ContentDomainObject>(context, c => c.Update(command));
        }

        protected async Task On(PatchContent command, CommandContext context)
        {
            await ValidateAsync(command, () => "Failed to patch content");

            await handler.UpdateAsync<ContentDomainObject>(context, c => c.Patch(command));
        }

        protected Task On(PublishContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Publish(command));
        }

        protected Task On(UnpublishContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Unpublish(command));
        }

        protected Task On(DeleteContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Delete(command));
        }

	    protected async Task On(CopyContent command, CommandContext context)
	    {
			Guard.NotNull(command.App, nameof(command.App));
		    Guard.NotEmpty(command.CopyFromId, nameof(command.App));
			Guard.NotNullOrEmpty(command.SchemaName, nameof(command.SchemaName));

			ISchemaEntity schemaEntity;

		    if (Guid.TryParse(command.SchemaName, out var schemaId))
		    {
			    schemaEntity = await schemas.FindSchemaByIdAsync(schemaId);
		    }
		    else
		    {
			    schemaEntity = await schemas.FindSchemaByNameAsync(command.App.Id, command.SchemaName);
		    }

			Guard.NotNull<NullReferenceException>(schemaEntity, $"'{nameof(schemaEntity)}' cannot be null. A schema could not be found by '{command.SchemaName}'");

		    var contentToCopy = await contentRepository.FindContentAsync(command.App, schemaEntity.Id, command.CopyFromId);

		    Guard.NotNull<NullReferenceException>(contentToCopy, $"'{nameof(contentToCopy)}' cannot be null. A content item with id '{command.CopyFromId}' could not be found in a schema with id `{schemaEntity.Id}`");

			var createCommand = new CreateContent
		    {
			    ContentId = command.ContentId,
				Data = contentToCopy.Data,
				Status = Status.Draft,
				AppId = command.AppId,
				Actor = command.Actor,
				SchemaId = command.SchemaId,
				ExpectedVersion = null
		    };

			await ValidateAsync(createCommand, () => "Failed to copy content", true);

			await handler.CreateAsync<ContentDomainObject>(context, c =>
			{
				c.Create(createCommand);

			    context.Complete(EntityCreatedResult.Create(createCommand.Data, c.Version));
		    });
		}

        protected Task On(SubmitContent command, CommandContext context)
        {
            return handler.UpdateAsync<ContentDomainObject>(context, c => c.Submit(command));
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }

        private async Task ValidateAsync(ContentDataCommand command, Func<string> message, bool enrich = false)
        {
            Guard.Valid(command, nameof(command), message);

            var taskForApp = appProvider.FindAppByIdAsync(command.AppId.Id);
            var taskForSchema = schemas.FindSchemaByIdAsync(command.SchemaId.Id);

            await Task.WhenAll(taskForApp, taskForSchema);

            var schemaObject = taskForSchema.Result.Schema;
            var schemaErrors = new List<ValidationError>();

            var appId = command.AppId.Id;

            var validationContext =
                new ValidationContext(
                    (contentIds, schemaId) =>
                    {
                        return contentRepository.QueryNotFoundAsync(appId, schemaId, contentIds.ToList());
                    },
                    assetIds =>
                    {
                        return assetRepository.QueryNotFoundAsync(appId, assetIds.ToList());
                    });

            await command.Data.ValidateAsync(validationContext, schemaObject, taskForApp.Result.PartitionResolver, schemaErrors);

            if (schemaErrors.Count > 0)
            {
                throw new ValidationException(message(), schemaErrors);
            }

            if (enrich)
            {
                command.Data.Enrich(schemaObject, taskForApp.Result.PartitionResolver);
            }
        }
    }
}
