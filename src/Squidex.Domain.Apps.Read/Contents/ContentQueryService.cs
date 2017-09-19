﻿// ==========================================================================
//  ContentQueryService.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.OData;
using Microsoft.OData.UriParser;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents.Edm;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Read.Contents
{
    public sealed class ContentQueryService : IContentQueryService
    {
        private readonly IContentRepository contentRepository;
        private readonly ISchemaProvider schemas;
        private readonly IScriptEngine scriptEngine;
        private readonly EdmModelBuilder modelBuilder;

        public ContentQueryService(
            IContentRepository contentRepository,
            ISchemaProvider schemas,
            IScriptEngine scriptEngine,
            EdmModelBuilder modelBuilder)
        {
            Guard.NotNull(contentRepository, nameof(contentRepository));
            Guard.NotNull(scriptEngine, nameof(scriptEngine));
            Guard.NotNull(modelBuilder, nameof(modelBuilder));
            Guard.NotNull(schemas, nameof(schemas));

            this.contentRepository = contentRepository;
            this.schemas = schemas;
            this.scriptEngine = scriptEngine;
            this.modelBuilder = modelBuilder;
        }

        public async Task<(ISchemaEntity Schema, IContentEntity Content)> FindContentAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, Guid id)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var isFrontendClient = user.IsInClient("squidex-frontend");

            var schema = await FindSchemaAsync(app, schemaIdOrName);

            var content = await contentRepository.FindContentAsync(app, schema, id);

            if (content == null || (content.Status != Status.Published && !isFrontendClient))
            {
                throw new DomainObjectNotFoundException(id.ToString(), typeof(ISchemaEntity));
            }

            content = TransformContent(user, schema, new List<IContentEntity> { content })[0];

            return (schema, content);
        }

        public async Task<(ISchemaEntity Schema, long Total, IReadOnlyList<IContentEntity> Items)> QueryWithCountAsync(IAppEntity app, string schemaIdOrName, ClaimsPrincipal user, bool archived, HashSet<Guid> ids, string query)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));
            Guard.NotNullOrEmpty(schemaIdOrName, nameof(schemaIdOrName));

            var schema = await FindSchemaAsync(app, schemaIdOrName);

            var parsedQuery = ParseQuery(app, query, schema);

            var status = new List<Status>();

            if (user.IsInClient("squidex-frontend"))
            {
                if (archived)
                {
                    status.Add(Status.Archived);
                }
                else
                {
                    status.Add(Status.Draft);
                    status.Add(Status.Declined);
                    status.Add(Status.Published);
                    status.Add(Status.Submitted);
                }
            }
            else
            {
                status.Add(Status.Published);
            }

            var taskForItems = contentRepository.QueryAsync(app, schema, status.ToArray(), ids, parsedQuery);
            var taskForCount = contentRepository.CountAsync(app, schema, status.ToArray(), ids, parsedQuery);

            await Task.WhenAll(taskForItems, taskForCount);

            var list = TransformContent(user, schema, taskForItems.Result.ToList());

            return (schema, taskForCount.Result, list);
        }

        public async Task<(IReadOnlyList<ISchemaEntity> Schemas, long Total, IReadOnlyList<IContentEntity> Items)> QueryWithCountAsync(IAppEntity app, ClaimsPrincipal user, HashSet<Guid> ids, string query)
        {
            Guard.NotNull(app, nameof(app));
            Guard.NotNull(user, nameof(user));

            var allSchemas = await schemas.QueryAllAsync(app.Id);

            var parsedQuery = ParseQuery(app, query, allSchemas.First());

            var status = new List<Status>();

            if (user.IsInClient("squidex-frontend"))
            {
                status.Add(Status.Draft);
                status.Add(Status.Declined);
                status.Add(Status.Published);
                status.Add(Status.Submitted);
            }
            else
            {
                status.Add(Status.Published);
            }

            var taskForItems = contentRepository.QueryAsync(app, allSchemas, status.ToArray(), ids, parsedQuery);
            var taskForCount = contentRepository.CountAsync(app, status.ToArray(), ids, parsedQuery);

            await Task.WhenAll(taskForItems, taskForCount);

            var list = TransformContent(user, allSchemas, taskForItems.Result.ToList());

            return (allSchemas, taskForCount.Result, list);
        }

        private List<IContentEntity> TransformContent(ClaimsPrincipal user, ISchemaEntity schema, List<IContentEntity> contents)
        {
            var scriptText = schema.ScriptQuery;

            if (!string.IsNullOrWhiteSpace(scriptText))
            {
                for (var i = 0; i < contents.Count; i++)
                {
                    var content = contents[i];
                    var contentData = scriptEngine.Transform(new ScriptContext { User = user, Data = content.Data, ContentId = content.Id }, scriptText);

                    contents[i] = SimpleMapper.Map(content, new Content { Data = contentData, SchemaId = content.SchemaId });
                }
            }

            return contents;
        }

        private List<IContentEntity> TransformContent(ClaimsPrincipal user, IEnumerable<ISchemaEntity> allSchemas, List<IContentEntity> contents)
        {
            for (int i = 0; i < allSchemas.Count(); i++)
            {
                var schema = allSchemas.ElementAt(i);

                TransformContent(user, schema, contents.Where(x => x.SchemaId == schema.Id).ToList());
            }

            return contents;
        }

        private ODataUriParser ParseQuery(IAppEntity app, string query, ISchemaEntity schema)
        {
            try
            {
                var model = modelBuilder.BuildEdmModel(schema, app);

                return model.ParseQuery(query);
            }
            catch (ODataException ex)
            {
                throw new ValidationException($"Failed to parse query: {ex.Message}", ex);
            }
        }

        public async Task<ISchemaEntity> FindSchemaAsync(IEntity app, string schemaIdOrName)
        {
            Guard.NotNull(app, nameof(app));

            ISchemaEntity schema = null;

            if (Guid.TryParse(schemaIdOrName, out var id))
            {
                schema = await schemas.FindSchemaByIdAsync(id);
            }

            if (schema == null)
            {
                schema = await schemas.FindSchemaByNameAsync(app.Id, schemaIdOrName);
            }

            if (schema == null)
            {
                throw new DomainObjectNotFoundException(schemaIdOrName, typeof(ISchemaEntity));
            }

            return schema;
        }

        private sealed class Content : IContentEntity
        {
            public Content()
            {
            }

            public Guid Id { get; set; }
            public Guid AppId { get; set; }

            public long Version { get; set; }

            public Instant Created { get; set; }
            public Instant LastModified { get; set; }

            public RefToken CreatedBy { get; set; }
            public RefToken LastModifiedBy { get; set; }

            public NamedContentData Data { get; set; }
            public Guid SchemaId { get; set; }

            public Status Status { get; set; }
        }
    }
}
