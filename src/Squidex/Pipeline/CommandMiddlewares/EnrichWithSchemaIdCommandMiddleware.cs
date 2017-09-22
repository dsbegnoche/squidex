// ==========================================================================
//  EnrichWithSchemaIdHandler.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Validators;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Domain.Apps.Write.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Pipeline.CommandMiddlewares
{
    public sealed class EnrichWithSchemaIdCommandMiddleware : ICommandMiddleware
    {
        private readonly ISchemaProvider schemas;
        private readonly IActionContextAccessor actionContextAccessor;

        public EnrichWithSchemaIdCommandMiddleware(ISchemaProvider schemas, IActionContextAccessor actionContextAccessor)
        {
            this.schemas = schemas;

            this.actionContextAccessor = actionContextAccessor;
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (context.Command is SchemaCommand schemaCommand && schemaCommand.SchemaId == null)
            {
                var routeValues = actionContextAccessor.ActionContext.RouteData.Values;

                if (routeValues.ContainsKey("name"))
                {
                    var schemaName = routeValues["name"].ToString();

                    ISchemaEntity schema;

                    if (Guid.TryParse(schemaName, out var id))
                    {
                        schema = await schemas.FindSchemaByIdAsync(id);
                    }
                    else
                    {
                        schema = await schemas.FindSchemaByNameAsync(schemaCommand.AppId.Id, schemaName);
                    }

                    if (schema == null)
                    {
                        throw new DomainObjectNotFoundException(schemaName, typeof(SchemaDomainObject));
                    }

                    schemaCommand.SchemaId = new NamedId<Guid>(schema.Id, schema.Name);
                }
            }

            if (context.Command is UpdatePattern updateCommand)
            {
                var all = await schemas.QueryAllAsync(updateCommand.AppId.Id);
                updateCommand.Schemas = new List<Guid>();
                foreach (var schema in all)
                {
                    var fieldsWithValidators = schema.SchemaDef.Fields
                        .Where(f => f.RawProperties is StringFieldProperties)
                        .Any(f => f.Validators.Any(v => v is PatternValidator));
                    if (fieldsWithValidators)
                    {
                        updateCommand.Schemas.Add(schema.Id);
                    }
                }
            }

            await next();
        }
    }
}
