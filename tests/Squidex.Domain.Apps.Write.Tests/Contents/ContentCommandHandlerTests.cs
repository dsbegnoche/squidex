// ==========================================================================
//  ContentCommandMiddlewareTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Apps.Services;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Domain.Apps.Read.Schemas.Services;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Shared.Identity;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentCommandMiddleware sut;
        private readonly ContentDomainObject content;
        private readonly ISchemaProvider schemaProvider = A.Fake<ISchemaProvider>();
        private readonly ISchemaEntity schemaEntity = A.Fake<ISchemaEntity>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly IAppEntity appEntity = A.Fake<IAppEntity>();
	    private readonly IContentRepository contentRepository = A.Fake<IContentRepository>();
		private readonly NamedContentData data = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(1));
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Create(Language.DE);
        private readonly Guid contentId = Guid.NewGuid();

        public ContentCommandMiddlewareTests()
        {
            var schema =
                Schema.Create("my-schema", new SchemaProperties())
                    .AddOrUpdateField(new NumberField(1, "my-field", Partitioning.Invariant,
                        new NumberFieldProperties { IsRequired = true }));

            content = new ContentDomainObject(contentId, -1);

            sut = new ContentCommandMiddleware(Handler, appProvider, A.Dummy<IAssetRepository>(), schemaProvider, contentRepository);

            A.CallTo(() => appEntity.LanguagesConfig).Returns(languagesConfig);
            A.CallTo(() => appEntity.PartitionResolver).Returns(languagesConfig.ToResolver());
            A.CallTo(() => appProvider.FindAppByIdAsync(AppId)).Returns(Task.FromResult(appEntity));

            A.CallTo(() => schemaEntity.Schema).Returns(schema);
            A.CallTo(() => schemaProvider.FindSchemaByIdAsync(SchemaId, false)).Returns(Task.FromResult(schemaEntity));
        }

        [Fact]
        public async Task Create_should_throw_exception_if_data_is_not_valid()
        {
            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = new NamedContentData() });

            await TestCreate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Create_should_create_content()
        {
            var context = CreateContextForCommand(new CreateContent { ContentId = contentId, Data = data });

            await TestCreate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });

            Assert.Equal(data, context.Result<EntityCreatedResult<NamedContentData>>().IdOrValue);
        }

        [Fact]
        public async Task Update_should_throw_exception_if_data_is_not_valid()
        {
            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = new NamedContentData() });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Update_should_update_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new UpdateContent { ContentId = contentId, Data = data });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Patch_should_throw_exception_if_data_is_not_valid()
        {
            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = new NamedContentData() });

            await TestUpdate(content, async _ =>
            {
                await Assert.ThrowsAsync<ValidationException>(() => sut.HandleAsync(context));
            }, false);
        }

        [Fact]
        public async Task Patch_should_update_domain_object()
        {
            var otherContent = new NamedContentData().AddField("my-field", new ContentFieldData().SetValue(3));

            CreateContent();

            var context = CreateContextForCommand(new PatchContent { ContentId = contentId, Data = otherContent });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Publish_should_publish_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new PublishContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Unpublish_should_unpublish_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new UnpublishContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

        [Fact]
        public async Task Delete_should_update_domain_object()
        {
            CreateContent();

            var command = CreateContextForCommand(new DeleteContent { ContentId = contentId });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(command);
            });
        }

	    [Fact]
	    public async Task Copy_should_throw_exception_if_app_is_not_provided()
	    {
		    var context = CreateContextForCommand(new CopyContent() { });

		    await TestCopy(content, async _ =>
		    {
			    await Assert.ThrowsAsync<ArgumentNullException>(() => sut.HandleAsync(context));
		    }, false);
	    }

	    [Fact]
	    public async Task Copy_should_throw_exception_if_CopyFromId_is_not_provided()
	    {
		    var context = CreateContextForCommand(new CopyContent() { App = appEntity });

		    await TestCopy(content, async _ =>
		    {
			    await Assert.ThrowsAsync<ArgumentException>(() => sut.HandleAsync(context));
		    }, false);
	    }

	    [Fact]
	    public async Task Copy_should_throw_exception_if_Schema_is_not_found()
	    {
		    A.CallTo(() => schemaProvider.FindSchemaByIdAsync(SchemaId, false)).Returns(Task.FromResult((ISchemaEntity)null));

		    var context = CreateContextForCommand(new CopyContent() { App = appEntity, CopyFromId = contentId });

		    await TestCopy(content, async _ =>
		    {
			    await Assert.ThrowsAsync<NullReferenceException>(() => sut.HandleAsync(context));
		    }, false);
	    }

	    [Fact]
	    public async Task Copy_should_throw_exception_if_content_to_copy_is_not_found()
	    {
		    A.CallTo(() => contentRepository.FindContentAsync(appEntity, SchemaId, contentId)).Returns(Task.FromResult((IContentEntity)null));

		    var context = CreateContextForCommand(new CopyContent() { App = appEntity, CopyFromId = contentId, SchemaId = SchemaNamedId });

		    await TestCopy(content, async _ =>
		    {
			    await Assert.ThrowsAsync<NullReferenceException>(() => sut.HandleAsync(context));
		    }, false);
	    }

	    [Fact]
	    public async Task Copy_should_create_content()
	    {
			IContentEntity copyFromContent = A.Fake<IContentEntity>();
		    A.CallTo(() => copyFromContent.Data).Returns(data);
		    A.CallTo(() => copyFromContent.Id).Returns(contentId);

		    A.CallTo(() => contentRepository.FindContentAsync(appEntity, SchemaId, contentId)).Returns(Task.FromResult(copyFromContent));


			var context = CreateContextForCommand(new CopyContent() { App = appEntity, CopyFromId = contentId, SchemaId = SchemaNamedId });

			await TestCopy(content, async _ =>
		    {
			    await sut.HandleAsync(context);
		    });

		    Assert.Equal(data, context.Result<EntityCreatedResult<NamedContentData>>().IdOrValue);
	    }

        [Fact]
        public async Task Submit_should_submit_domain_object()
        {
            CreateContent();

            var context = CreateContextForCommand(new SubmitContent
            {
                ContentId = contentId,
                Roles = new List<string> {SquidexRoles.AppAuthor}
            });

            await TestUpdate(content, async _ =>
            {
                await sut.HandleAsync(context);
            });
        }

		private ContentDomainObject CreateContent()
        {
            return content.Create(new CreateContent { Data = data });
        }
    }
}
