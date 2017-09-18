// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

namespace Squidex.Tests.Controllers.ContentApi
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Routing;
    using Moq;
    using Squidex.Config;
    using Squidex.Controllers.ContentApi;
    using Squidex.Domain.Apps.Core;
    using Squidex.Domain.Apps.Core.Contents;
    using Squidex.Domain.Apps.Core.Schemas;
    using Squidex.Domain.Apps.Read.Apps;
    using Squidex.Domain.Apps.Read.Contents;
    using Squidex.Domain.Apps.Read.Contents.GraphQL;
    using Squidex.Domain.Apps.Read.Schemas;
    using Squidex.Domain.Apps.Write.Contents;
    using Squidex.Infrastructure;
    using Squidex.Infrastructure.CQRS.Commands;
    using Squidex.Infrastructure.Security;
    using Squidex.Infrastructure.UsageTracking;
    using Squidex.Pipeline;
    using Xunit;

    public class ContentsControllerTests
    {
        private readonly Mock<IContentQueryService> contentQuery = new Mock<IContentQueryService>();
        private readonly Mock<IContentVersionLoader> contentVersionLoader = new Mock<IContentVersionLoader>();
        private readonly Mock<IGraphQLService> graphQl = new Mock<IGraphQLService>();
        private readonly Mock<IContentUsageTracker> contentUsageTracker = new Mock<IContentUsageTracker>();
        private readonly Mock<ClaimsPrincipal> user = new Mock<ClaimsPrincipal>();
        private readonly Mock<IAppEntity> appEntity = new Mock<IAppEntity>();

        private readonly Mock<HttpContext> httpContext = new Mock<HttpContext>();
        private readonly Mock<HttpResponse> httpResponse = new Mock<HttpResponse>();
        private readonly Mock<HttpRequest> httpRequest = new Mock<HttpRequest>();

        private readonly Squidex.Controllers.ContentApi.ContentsController systemUnderTest;

        public ContentsControllerTests()
        {
            this.systemUnderTest = new ContentsController(
                new Mock<ICommandBus>().Object,
                this.contentUsageTracker.Object,
                this.contentQuery.Object,
                this.contentVersionLoader.Object,
                this.graphQl.Object
                );

            this.user.Setup(p => p.Claims).Returns(new List<Claim>()
            {
                new Claim(OpenIdClaims.ClientId, Constants.FrontendClient)
            });

            this.appEntity.Setup(x => x.LanguagesConfig).Returns(LanguagesConfig.Create(Language.EN));

            this.httpContext.SetupGet(x => x.User).Returns(this.user.Object);
            this.httpContext.Setup(x => x.Features.Get<IAppFeature>())
                .Returns(new AppFeature(this.appEntity.Object));

            this.httpRequest.Setup(x => x.QueryString).Returns(new QueryString("?test=1"));

            this.httpResponse.Setup(x => x.Headers).Returns(new Mock<IHeaderDictionary>().Object);

            this.httpContext.Setup(x => x.Response).Returns(this.httpResponse.Object);
            this.httpContext.Setup(x => x.Request).Returns(this.httpRequest.Object);

            this.systemUnderTest.ControllerContext = new ControllerContext(new ActionContext(this.httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
        }

        [Fact]
        public async Task GetContent_tracks_content_usage()
        {
            // Arrange
            Guid contentId = Guid.NewGuid();
            Mock<ISchemaEntity> schema = new Mock<ISchemaEntity>();
            Mock<IContentEntity> content = new Mock<IContentEntity>();

            content.Setup(x => x.Data).Returns(new NamedContentData());
            schema.Setup(x => x.SchemaDef)
                .Returns(new Schema("test", false, new SchemaProperties(), ImmutableList<Field>.Empty));

            this.user.Setup(p => p.Claims).Returns(new List<Claim>()
            {
                new Claim(OpenIdClaims.ClientId, "app1")
            });

            this.contentQuery.Setup(x => x.FindContentAsync(It.IsAny<IAppEntity>(), It.IsAny<string>(),
                    this.user.Object, contentId))
                .ReturnsAsync((schema.Object, content.Object));

            // Act
            var result = await this.systemUnderTest.GetContent("test", contentId);

            // Assert
            this.contentUsageTracker.Verify(x => x.TrackAsync(It.IsAny<List<Guid>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetContent__does_not_track_content_usage()
        {
            // Arrange
            Guid contentId = Guid.NewGuid();
            Mock<ISchemaEntity> schema = new Mock<ISchemaEntity>();
            Mock<IContentEntity> content = new Mock<IContentEntity>();

            content.Setup(x => x.Data).Returns(new NamedContentData());
            schema.Setup(x => x.SchemaDef)
                .Returns(new Schema("test", false, new SchemaProperties(), ImmutableList<Field>.Empty));

            this.contentQuery.Setup(x => x.FindContentAsync(It.IsAny<IAppEntity>(), It.IsAny<string>(),
                    this.user.Object, contentId))
                .ReturnsAsync((schema.Object, content.Object));

            // Act
            var result = await this.systemUnderTest.GetContent("test", contentId);

            // Assert
            this.contentUsageTracker.Verify(x => x.TrackAsync(It.IsAny<List<Guid>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetContents_tracks_content_usage()
        {
            // Arrange
            Guid contentId = Guid.NewGuid();
            Mock<ISchemaEntity> schema = new Mock<ISchemaEntity>();
            Mock<IContentEntity> content = new Mock<IContentEntity>();

            content.Setup(x => x.Data).Returns(new NamedContentData());
            schema.Setup(x => x.SchemaDef)
                .Returns(new Schema("test", false, new SchemaProperties(), ImmutableList<Field>.Empty));

            this.user.Setup(p => p.Claims).Returns(new List<Claim>()
            {
                new Claim(OpenIdClaims.ClientId, "app1")
            });

            this.contentQuery.Setup(x => x.QueryWithCountAsync(It.IsAny<IAppEntity>(), It.IsAny<string>(), this.user.Object, false, new HashSet<Guid>() {contentId}, It.IsAny<string>()))
                .ReturnsAsync((schema.Object, 1, new List<IContentEntity>() { content.Object }.AsReadOnly()));

            // Act
            var result = await this.systemUnderTest.GetContents("test", false, contentId.ToString());

            // Assert
            this.contentUsageTracker.Verify(x => x.TrackAsync(It.IsAny<List<Guid>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Once);
        }

        [Fact]
        public async Task GetContents__does_not_track_content_usage()
        {
            // Arrange
            Guid contentId = Guid.NewGuid();
            Mock<ISchemaEntity> schema = new Mock<ISchemaEntity>();
            Mock<IContentEntity> content = new Mock<IContentEntity>();

            content.Setup(x => x.Data).Returns(new NamedContentData());
            schema.Setup(x => x.SchemaDef)
                .Returns(new Schema("test", false, new SchemaProperties(), ImmutableList<Field>.Empty));

            this.contentQuery.Setup(x => x.QueryWithCountAsync(It.IsAny<IAppEntity>(), It.IsAny<string>(), this.user.Object, false, new HashSet<Guid>() { contentId }, It.IsAny<string>()))
                .ReturnsAsync((schema.Object, 1, new List<IContentEntity>() { content.Object }.AsReadOnly()));

            // Act
            var result = await this.systemUnderTest.GetContents("test", false, contentId.ToString());

            // Assert
            this.contentUsageTracker.Verify(x => x.TrackAsync(It.IsAny<List<Guid>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetContents_across_schemas_tracks_content_usage()
        {
            // Arrange
            Guid contentId = Guid.NewGuid();
            Mock<ISchemaEntity> schema = new Mock<ISchemaEntity>();
            Mock<IContentEntity> content = new Mock<IContentEntity>();

            content.Setup(x => x.Data).Returns(new NamedContentData());
            schema.Setup(x => x.SchemaDef)
                .Returns(new Schema("test", false, new SchemaProperties(), ImmutableList<Field>.Empty));

            this.user.Setup(p => p.Claims).Returns(new List<Claim>()
            {
                new Claim(OpenIdClaims.ClientId, "app1")
            });

            this.contentQuery.Setup(x => x.QueryWithCountAsync(It.IsAny<IAppEntity>(), this.user.Object, new HashSet<Guid>() { contentId }))
                .ReturnsAsync((new List<ISchemaEntity> { schema.Object }.AsReadOnly(), 1, new List<IContentEntity>() { content.Object }.AsReadOnly()));

            // Act
            var result = await this.systemUnderTest.GetContentsFromAllSchemas(contentId.ToString());

            // Assert
            this.contentUsageTracker.Verify(x => x.TrackAsync(It.IsAny<List<Guid>>(), It.IsAny<DateTime>(), It.IsAny<Guid>()), Times.Once);
        }
    }
}
