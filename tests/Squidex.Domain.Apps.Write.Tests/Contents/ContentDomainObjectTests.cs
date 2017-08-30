// ==========================================================================
//  ContentDomainObjectTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using FluentAssertions;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Domain.Apps.Write.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Shared.Identity;
using Xunit;

// ReSharper disable ConvertToConstant.Local

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentDomainObjectTests : HandlerTestBase<ContentDomainObject>
    {
        private readonly ContentDomainObject sut;
        private readonly NamedContentData data =
            new NamedContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddValue("iv", 1));
        private readonly NamedContentData otherData =
            new NamedContentData()
                .AddField("field2",
                    new ContentFieldData()
                        .AddValue("iv", 2));

        public Guid ContentId { get; } = Guid.NewGuid();

        public ContentDomainObjectTests()
        {
            sut = new ContentDomainObject(ContentId, 0);
        }

        [Fact]
        public void Create_should_throw_exception_if_created()
        {
            sut.Create(new CreateContent { Data = data });

            Assert.Throws<DomainException>(() =>
            {
                sut.Create(CreateContentCommand(new CreateContent { Data = data }));
            });
        }

        [Fact]
        public void Create_should_throw_exception_if_command_is_not_valid()
        {
            Assert.Throws<ValidationException>(() =>
            {
                sut.Create(CreateContentCommand(new CreateContent()));
            });
        }

        [Fact]
        public void Create_should_create_events()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data })
                );
        }

        [Fact]
        public void Create_should_also_publish_if_set_to_true()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data, Status = Core.Apps.Status.Published }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data }),
                    CreateContentEvent(new ContentPublished())
                );
        }

        [Fact]
        public void Create_should_sumbit_if_status_submitted()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data, Status = Status.Submitted}));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentCreated { Data = data }),
                    CreateContentEvent(new ContentSubmitted())
                );
        }

        [Fact]
        public void Update_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateContentCommand(new UpdateContent { Data = data }));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Update(CreateContentCommand(new UpdateContent()));
            });
        }

        [Fact]
        public void Update_should_throw_exception_if_command_is_not_valid()
        {
            CreateContent();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Update(CreateContentCommand(new UpdateContent()));
            });
        }

        [Fact]
        public void Update_should_create_events()
        {
            CreateContent();

            sut.Update(CreateContentCommand(new UpdateContent { Data = otherData }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );
        }

        [Fact]
        public void Update_should_create_events_if_submitted_and_user_is_author()
        {
            CreateContent();
            SubmitContent();

            sut.Update(CreateContentCommandAuthor(new UpdateContent { Data = otherData }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );
        }

        [Fact]
        public void Update_should_not_create_event_for_same_data()
        {
            CreateContent();
            UpdateContent();

            sut.Update(CreateContentCommand(new UpdateContent { Data = data }));

            sut.GetUncomittedEvents().Should().BeEmpty();
        }

        [Fact]
        public void Update_should_throw_exception_if_published_and_user_is_author()
        {
            CreateContent();
            PublishContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Update(CreateContentCommandAuthor(new UpdateContent { Data = data }));
            });
        }

        [Fact]
        public void Patch_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent { Data = data }));
            });
        }

        [Fact]
        public void Patch_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent()));
            });
        }

        [Fact]
        public void Patch_should_throw_exception_if_command_is_not_valid()
        {
            CreateContent();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Patch(CreateContentCommand(new PatchContent()));
            });
        }

        [Fact]
        public void Patch_should_create_events()
        {
            CreateContent();

            sut.Patch(CreateContentCommand(new PatchContent { Data = otherData }));

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUpdated { Data = otherData })
                );
        }

        [Fact]
        public void Patch_should_not_create_event_for_same_data()
        {
            CreateContent();
            UpdateContent();

            sut.Patch(CreateContentCommand(new PatchContent { Data = data }));

            sut.GetUncomittedEvents().Should().BeEmpty();
        }

        [Fact]
        public void Patch_should_throw_exception_if_published_and_user_is_author()
        {
            CreateContent();
            PublishContent();

            Assert.Throws<ValidationException>(() =>
            {
                sut.Patch(CreateContentCommandAuthor(new PatchContent()));
            });
        }

        [Fact]
        public void Publish_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateContentCommand(new PublishContent()));
            });
        }

        [Fact]
        public void Publish_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateContentCommand(new PublishContent()));
            });
        }

        [Fact]
        public void Publish_should_throw_exception_if_user_not_editor_or_greater()
        {
            CreateContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Publish(CreateContentCommandAuthor(new PublishContent()));
            });
        }

        [Fact]
        public void Publish_should_refresh_properties_and_create_events()
        {
            CreateContent();

            sut.Publish(CreateContentCommand(new PublishContent()));

            Assert.True(sut.IsPublished);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentPublished())
                );
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateContentCommand(new UnpublishContent()));
            });
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateContentCommand(new UnpublishContent()));
            });
        }

        [Fact]
        public void Unpublish_should_refresh_properties_and_create_events()
        {
            CreateContent();
            PublishContent();

            sut.Unpublish(CreateContentCommand(new UnpublishContent()));

            Assert.False(sut.IsPublished);
            Assert.Equal(Status.Draft, sut.Status);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentUnpublished())
                );
        }

        [Fact]
        public void Unpublish_should_throw_exception_if_submitted()
        {
            CreateContent();
            SubmitContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Unpublish(CreateContentCommandAuthor(new UnpublishContent()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateContentCommand(new DeleteContent()));
            });
        }

        [Fact]
        public void Delete_should_throw_exception_if_already_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateContentCommand(new DeleteContent()));
            });
        }

        [Fact]
        public void Delete_should_update_properties_create_events()
        {
            CreateContent();

            sut.Delete(CreateContentCommand(new DeleteContent()));

            Assert.True(sut.IsDeleted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );
        }

        [Fact]
        public void Delete_should_throw_exception_if_user_is_not_editor_or_greater()
        {
            CreateContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Delete(CreateContentCommandAuthor(new DeleteContent()));
            });
        }

        [Fact]
        public void Submit_should_throw_exception_if_content_is_deleted()
        {
            CreateContent();
            DeleteContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Submit(CreateContentCommand(new SubmitContent()));
            });
        }

        [Fact]
        public void Submit_should_throw_exception_if_content_is_published()
        {
            CreateContent();
            PublishContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Submit(CreateContentCommand(new SubmitContent()));
            });
        }

        [Fact]
        public void Submit_should_throw_exception_if_content_is_not_created()
        {
            Assert.Throws<DomainException>(() =>
            {
                sut.Submit(CreateContentCommand(new SubmitContent()));
            });
        }

        [Fact]
        public void Submit_should_refresh_properties_and_create_events()
        {
            CreateContent();

            sut.Submit(CreateContentCommandAuthor(new SubmitContent()));

            Assert.True(sut.Status == Status.Submitted);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentSubmitted())
                );
        }

        [Fact]
        public void Submit_should_throw_exception_if_content_is_already_submitted()
        {
            CreateContent();
            SubmitContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Submit(CreateContentCommandAuthor(new SubmitContent()));
            });
        }

        [Fact]
        public void Submit_should_throw_exception_if_user_is_not_author()
        {
            CreateContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Submit(CreateContentCommandReader(new SubmitContent()));
            });
        }

        [Fact]
        public void Submit_should_throw_exception_if_user_is_owner()
        {
            CreateContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Submit(CreateContentCommand(new SubmitContent()));
            });
        }

        [Fact]
        public void Decline_should_refresh_properties_and_create_events()
        {
            CreateContent();
            SubmitContent();

            sut.Decline(CreateContentCommand(new DeclineContent()));

            Assert.True(sut.Status == Status.Declined);

            sut.GetUncomittedEvents()
                .ShouldHaveSameEvents(
                    CreateContentEvent(new ContentDeleted())
                );
        }

        [Fact]
        public void Decline_should_throw_exception_if_user_is_not_editor_or_higher()
        {
            CreateContent();

            Assert.Throws<DomainException>(() =>
            {
                sut.Decline(CreateContentCommandAuthor(new DeclineContent()));
            });
        }

        private void CreateContent()
        {
            sut.Create(CreateContentCommand(new CreateContent { Data = data }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void UpdateContent()
        {
            sut.Update(CreateContentCommand(new UpdateContent { Data = data }));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void PublishContent()
        {
            sut.Publish(CreateContentCommand(new PublishContent()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void DeleteContent()
        {
            sut.Delete(CreateContentCommand(new DeleteContent()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        private void SubmitContent()
        {
            sut.Submit(CreateContentCommandAuthor(new SubmitContent()));

            ((IAggregate)sut).ClearUncommittedEvents();
        }

        protected T CreateContentEvent<T>(T @event) where T : ContentEvent
        {
            @event.ContentId = ContentId;

            return CreateEvent(@event);
        }

        /// <summary> This creates command with owner roles </summary>
        protected T CreateContentCommand<T>(T command) where T : ContentCommand
        {
            command.ContentId = ContentId;

            return CreateCommand(command);
        }

        /// <summary>
        /// Creates content command with reader roles.
        /// </summary>
        protected T CreateContentCommandReader<T>(T command) where T : ContentCommand
        {
            command.ContentId = ContentId;
            command.Roles = new List<string> { SquidexRoles.AppReader };

            return CreateCommand(command);
        }

        /// <summary>
        /// Creates content command with author roles.
        /// </summary>
        protected T CreateContentCommandAuthor<T>(T command) where T : ContentCommand
        {
            command.ContentId = ContentId;
            command.Roles = new List<string> { SquidexRoles.AppReader, SquidexRoles.AppAuthor };

            return CreateCommand(command);
        }
    }
}
