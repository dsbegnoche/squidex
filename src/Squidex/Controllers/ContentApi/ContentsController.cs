// ==========================================================================
//  ContentsController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.Extensions.Primitives;
using NSwag.Annotations;
using Squidex.Controllers.ContentApi.Models;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.GraphQL;
using Squidex.Domain.Apps.Write.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Reflection;
using Squidex.Pipeline;

namespace Squidex.Controllers.ContentApi
{
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerIgnore]
    public sealed class ContentsController : ControllerBase
    {
        private readonly IContentQueryService contentQuery;
        private readonly IGraphQLService graphQl;

        public ContentsController(ICommandBus commandBus, IContentQueryService contentQuery, IGraphQLService graphQl)
            : base(commandBus)
        {
            this.contentQuery = contentQuery;

            this.graphQl = graphQl;
        }

        [MustBeAppReader]
        [HttpGet]
        [HttpPost]
        [Route("content/{app}/graphql")]
        [ApiCosts(2)]
        public async Task<IActionResult> PostGraphQL([FromBody] GraphQLQuery query)
        {
            var result = await graphQl.QueryAsync(App, User, query);

            if (result.Errors?.Length > 0)
            {
                return BadRequest(new { result.Data, result.Errors });
            }
            else
            {
                return Ok(new { result.Data });
            }
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}")]
        [ApiCosts(2)]
        public async Task<IActionResult> GetContents(string name, [FromQuery] string ids = null)
        {
            var idsList = new HashSet<Guid>();

            if (!string.IsNullOrWhiteSpace(ids))
            {
                foreach (var id in ids.Split(','))
                {
                    if (Guid.TryParse(id, out var guid))
                    {
                        idsList.Add(guid);
                    }
                }
            }

            var isFrontendClient = User.IsFrontendClient();

            var contents = await contentQuery.QueryWithCountAsync(App, name, User, idsList, Request.QueryString.ToString());

            var response = new AssetsDto
            {
                Total = contents.Total,
                Items = contents.Items.Take(200).Select(item =>
                {
                    var itemModel = SimpleMapper.Map(item, new ContentDto());

                    if (item.Data != null)
                    {
                        itemModel.Data = item.Data.ToApiModel(contents.Schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
                    }

                    return itemModel;
                }).ToArray()
            };

            return Ok(response);
        }

        [MustBeAppReader]
        [HttpGet]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> GetContent(string name, Guid id)
        {
            var content = await contentQuery.FindContentAsync(App, name, User, id);

            var response = SimpleMapper.Map(content.Content, new ContentDto());

            if (content.Content.Data != null)
            {
                var isFrontendClient = User.IsFrontendClient();

                response.Data = content.Content.Data.ToApiModel(content.Schema.SchemaDef, App.LanguagesConfig, !isFrontendClient);
            }

            Response.Headers["ETag"] = new StringValues(content.Content.Version.ToString());

            return Ok(response);
        }

        [MustBeAppAuthor]
        [HttpPost]
        [Route("content/{app}/{name}/")]
        [ApiCosts(1)]
        public async Task<IActionResult> PostContent([FromBody] NamedContentData request, [FromQuery] Status status = Status.Draft)
        {
            var command = new CreateContent { ContentId = Guid.NewGuid(), Data = request.ToCleaned(), Status = status };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<EntityCreatedResult<NamedContentData>>();
            var response = ContentDto.Create(command, result);

            return CreatedAtAction(nameof(GetContent), new { id = command.ContentId }, response);
        }

        [MustBeAppAuthor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PutContent(string name, Guid id, [FromBody] NamedContentData request)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new UpdateContent { ContentId = id, User = User, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppAuthor]
        [HttpPatch]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> PatchContent(string name, Guid id, [FromBody] NamedContentData request)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new PatchContent { ContentId = id, User = User, Data = request.ToCleaned() };

            var context = await CommandBus.PublishAsync(command);

            var result = context.Result<ContentDataChangedResult>();
            var response = result.Data;

            return Ok(response);
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/publish")]
        [ApiCosts(1)]
        public async Task<IActionResult> PublishContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new PublishContent { ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/unpublish")]
        [ApiCosts(1)]
        public async Task<IActionResult> UnpublishContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new UnpublishContent { ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppAuthor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/submit")]
        [ApiCosts(1)]
        public async Task<IActionResult> SubmitContent(Guid id)
        {
            var command = new SubmitContent() { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppEditor]
        [HttpPut]
        [Route("content/{app}/{name}/{id}/decline")]
        [ApiCosts(1)]
        public async Task<IActionResult> Decline(Guid id)
        {
            var command = new DeclineContent() { ContentId = id };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }

        [MustBeAppAuthor]
        [HttpDelete]
        [Route("content/{app}/{name}/{id}")]
        [ApiCosts(1)]
        public async Task<IActionResult> DeleteContent(string name, Guid id)
        {
            await contentQuery.FindSchemaAsync(App, name);

            var command = new DeleteContent { ContentId = id, User = User };

            await CommandBus.PublishAsync(command);

            return NoContent();
        }
    }
}
