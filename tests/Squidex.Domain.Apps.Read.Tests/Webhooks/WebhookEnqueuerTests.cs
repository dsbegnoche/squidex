// ==========================================================================
//  WebhookEnqueuerTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Domain.Apps.Core.Webhooks;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Xunit;

// ReSharper disable MethodSupportsCancellation
// ReSharper disable ImplicitlyCapturedClosure
// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Read.Webhooks
{
    public class WebhookEnqueuerTests
    {
        private readonly IClock clock = A.Fake<IClock>();
        private readonly IWebhookRepository webhookRepository = A.Fake<IWebhookRepository>();
        private readonly IWebhookEventRepository webhookEventRepository = A.Fake<IWebhookEventRepository>();
        private readonly TypeNameRegistry typeNameRegisty = new TypeNameRegistry();
        private readonly Instant now = SystemClock.Instance.GetCurrentInstant();
        private readonly NamedId<Guid> appId = new NamedId<Guid>(Guid.NewGuid(), "my-app");
        private readonly NamedId<Guid> schemaId = new NamedId<Guid>(Guid.NewGuid(), "my-schema");
        private readonly WebhookEnqueuer sut;

        public WebhookEnqueuerTests()
        {
            A.CallTo(() => clock.GetCurrentInstant()).Returns(now);

            typeNameRegisty.Map(typeof(ContentCreated));
            typeNameRegisty.Map(typeof(ContentUpdated));
            typeNameRegisty.Map(typeof(ContentPublished));
            typeNameRegisty.Map(typeof(ContentUnpublished));
            typeNameRegisty.Map(typeof(ContentDeleted));
            typeNameRegisty.Map(typeof(ContentSubmitted));

            sut = new WebhookEnqueuer(
                typeNameRegisty,
                webhookEventRepository,
                webhookRepository,
                clock, new JsonSerializer());
        }

        [Fact]
        public void Should_return_contents_filter_for_events_filter()
        {
            Assert.Equal("^content-", sut.EventsFilter);
        }

        [Fact]
        public void Should_return_type_name_for_name()
        {
            Assert.Equal(typeof(WebhookEnqueuer).Name, sut.Name);
        }

        [Fact]
        public Task Should_do_nothing_on_clear()
        {
            return sut.ClearAsync();
        }

        [Fact]
        public async Task Should_update_repositories_on_successful_requests()
        {

            var @event = Envelope.Create(new ContentCreated { AppId = appId, SchemaId = schemaId });

            var webhook1 = CreateWebhook(1);
            var webhook2 = CreateWebhook(2);

            A.CallTo(() => webhookRepository.QueryByAppAsync(appId.Id))
                .Returns(new List<IWebhookEntity> { webhook1, webhook2 });

            await sut.On(@event);

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                 && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                 && webhookJob.Id != Guid.Empty
                 && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                 && webhookJob.AppId == appId.Id
                 && webhookJob.EventName == "MySchemaCreatedEvent"
                 && webhookJob.RequestUrl == webhook1.Url
                 && webhookJob.WebhookId == webhook1.Id), now)).MustHaveHappened();

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                    && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                    && webhookJob.Id != Guid.Empty
                    && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                    && webhookJob.AppId == appId.Id
                    && webhookJob.EventName == "MySchemaCreatedEvent"
                    && webhookJob.RequestUrl == webhook2.Url
                    && webhookJob.WebhookId == webhook2.Id), now)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_update_event_on_content_updated()
        {

            var @event = Envelope.Create(new ContentUpdated { AppId = appId, SchemaId = schemaId });

            var webhook1 = CreateWebhook(1);
            var webhook2 = CreateWebhook(2);

            A.CallTo(() => webhookRepository.QueryByAppAsync(appId.Id))
                .Returns(Task.FromResult<IReadOnlyList<IWebhookEntity>>(new List<IWebhookEntity> { webhook1, webhook2 }));

            await sut.On(@event);

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                    && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                    && webhookJob.Id != Guid.Empty
                    && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                    && webhookJob.AppId == appId.Id
                    && webhookJob.EventName == "MySchemaUpdatedEvent"
                    && webhookJob.RequestUrl == webhook2.Url
                    && webhookJob.WebhookId == webhook2.Id), now)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_delete_event_on_content_deleted()
        {

            var @event = Envelope.Create(new ContentDeleted { AppId = appId, SchemaId = schemaId });

            var webhook1 = CreateWebhook(1);
            var webhook2 = CreateWebhook(2);

            A.CallTo(() => webhookRepository.QueryByAppAsync(appId.Id))
                .Returns(Task.FromResult<IReadOnlyList<IWebhookEntity>>(new List<IWebhookEntity> { webhook1, webhook2 }));

            await sut.On(@event);

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                    && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                    && webhookJob.Id != Guid.Empty
                    && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                    && webhookJob.AppId == appId.Id
                    && webhookJob.EventName == "MySchemaDeletedEvent"
                    && webhookJob.RequestUrl == webhook2.Url
                    && webhookJob.WebhookId == webhook2.Id), now)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_published_event_on_content_published()
        {

            var @event = Envelope.Create(new ContentPublished { AppId = appId, SchemaId = schemaId });

            var webhook1 = CreateWebhook(1);
            var webhook2 = CreateWebhook(2);

            A.CallTo(() => webhookRepository.QueryByAppAsync(appId.Id))
                .Returns(Task.FromResult<IReadOnlyList<IWebhookEntity>>(new List<IWebhookEntity> { webhook1, webhook2 }));

            await sut.On(@event);

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                    && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                    && webhookJob.Id != Guid.Empty
                    && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                    && webhookJob.AppId == appId.Id
                    && webhookJob.EventName == "MySchemaPublishedEvent"
                    && webhookJob.RequestUrl == webhook2.Url
                    && webhookJob.WebhookId == webhook2.Id), now)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_unpublish_event_on_content_unpublished()
        {

            var @event = Envelope.Create(new ContentUnpublished { AppId = appId, SchemaId = schemaId });

            var webhook1 = CreateWebhook(1);
            var webhook2 = CreateWebhook(2);

            A.CallTo(() => webhookRepository.QueryByAppAsync(appId.Id))
                .Returns(Task.FromResult<IReadOnlyList<IWebhookEntity>>(new List<IWebhookEntity> { webhook1, webhook2 }));

            await sut.On(@event);

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                    && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                    && webhookJob.Id != Guid.Empty
                    && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                    && webhookJob.AppId == appId.Id
                    && webhookJob.EventName == "MySchemaUnpublishedEvent"
                    && webhookJob.RequestUrl == webhook2.Url
                    && webhookJob.WebhookId == webhook2.Id), now)).MustHaveHappened();
        }

        [Fact]
        public async Task Should_send_submit_event_on_content_submitted()
        {

            var @event = Envelope.Create(new ContentSubmitted { AppId = appId, SchemaId = schemaId });

            var webhook1 = CreateWebhook(1);
            var webhook2 = CreateWebhook(2);

            A.CallTo(() => webhookRepository.QueryByAppAsync(appId.Id))
                .Returns(Task.FromResult<IReadOnlyList<IWebhookEntity>>(new List<IWebhookEntity> { webhook1, webhook2 }));

            await sut.On(@event);

            A.CallTo(() => webhookEventRepository.EnqueueAsync(
                A<WebhookJob>.That.Matches(webhookJob =>
                    !string.IsNullOrWhiteSpace(webhookJob.RequestSignature)
                    && !string.IsNullOrWhiteSpace(webhookJob.RequestBody)
                    && webhookJob.Id != Guid.Empty
                    && webhookJob.Expires == now.Plus(Duration.FromDays(2))
                    && webhookJob.AppId == appId.Id
                    && webhookJob.EventName == "MySchemaSubmittedEvent"
                    && webhookJob.RequestUrl == webhook2.Url
                    && webhookJob.WebhookId == webhook2.Id), now)).MustHaveHappened();
        }

        private IWebhookEntity CreateWebhook(int offset)
        {
            var webhook = A.Dummy<IWebhookEntity>();

            var schema = new WebhookSchema
            {
                SchemaId = schemaId.Id,
                SendCreate = true,
                SendUpdate = true,
                SendPublish = true,
                SendDelete = true,
                SendUnpublish = true,
                SendSubmit = true
            };

            A.CallTo(() => webhook.Id).Returns(Guid.NewGuid());
            A.CallTo(() => webhook.Url).Returns(new Uri($"http://domain{offset}.com"));
            A.CallTo(() => webhook.Schemas).Returns(new[] { schema });
            A.CallTo(() => webhook.SharedSecret).Returns($"secret{offset}");

            return webhook;
        }
    }
}
