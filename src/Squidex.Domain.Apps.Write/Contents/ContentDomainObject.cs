// ==========================================================================
//  ContentDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Write.Contents
{
    public class ContentDomainObject : DomainObjectBase
    {
        private bool isCreated;
        private Status status;

        private NamedContentData data;

        public bool IsDeleted
        {
            get { return status == Status.Deleted; }
        }

        public bool IsPublished
        {
            get { return status == Status.Published; }
        }

        public bool IsSubmitted
        {
            get { return status == Status.Submitted; }
        }

        public Status Status
        {
            get { return status; }
        }

        public ContentDomainObject(Guid id, int version)
            : base(id, version)
        {
        }

        protected void On(ContentCreated @event)
        {
            isCreated = true;

            data = @event.Data;

            status = Status.Draft;
        }

        protected void On(ContentUpdated @event)
        {
            data = @event.Data;
        }

        protected void On(ContentPublished @event)
        {
            status = Status.Published;
        }

        protected void On(ContentUnpublished @event)
        {
            status = Status.Draft;
        }

        protected void On(ContentDeleted @event)
        {
            status = Status.Deleted;
        }

        protected void On(ContentSubmitted @event)
        {
            status = Status.Submitted;
        }

        public ContentDomainObject Create(CreateContent command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create content");

            VerifyNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new ContentCreated()));

            if (command.Status == Status.Published)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentPublished()));
            }
            else if (command.Status == Status.Submitted)
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentSubmitted()));
            }

            return this;
        }

        public ContentDomainObject Delete(DeleteContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyIsEditor(command);
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentDeleted()));

            return this;
        }

        public ContentDomainObject Publish(PublishContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyIsEditor(command);
            VerifyCreatedAndNotDeleted();

            RaiseEvent(SimpleMapper.Map(command, new ContentPublished()));

            return this;
        }

        public ContentDomainObject Unpublish(UnpublishContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyCreatedAndNotDeleted();
            VerifyNotSubmitted();

            RaiseEvent(SimpleMapper.Map(command, new ContentUnpublished()));

            return this;
        }

        public ContentDomainObject Update(UpdateContent command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update content");

            VerifyCreatedAndNotDeleted();
            if (Status == Status.Published)
            {
                VerifyIsEditor(command);
            }

            if (!command.Data.Equals(data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated()));
            }

            return this;
        }

        public ContentDomainObject Patch(PatchContent command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot patch content");

            VerifyCreatedAndNotDeleted();
            if (Status == Status.Published)
            {
                VerifyIsEditor(command);
            }

            var newData = data.MergeInto(command.Data);

            if (!newData.Equals(data))
            {
                RaiseEvent(SimpleMapper.Map(command, new ContentUpdated { Data = newData }));
            }

            return this;
        }

        public ContentDomainObject Submit(SubmitContent command)
        {
            Guard.NotNull(command, nameof(command));

            VerifyHighestRightsAuthor(command);
            VerifyCreatedAndNotDeleted();
            VerifyNotPublished();
            VerifyNotSubmitted();

            RaiseEvent(SimpleMapper.Map(command, new ContentSubmitted()));

            return this;
        }

        private void VerifyNotCreated()
        {
            if (isCreated)
            {
                throw new DomainException("Content has already been created.");
            }
        }

        private void VerifyCreatedAndNotDeleted()
        {
            if (IsDeleted || !isCreated)
            {
                throw new DomainException("Content has already been deleted or not created yet.");
            }
        }

        private void VerifyNotPublished()
        {
            if (IsPublished)
            {
                throw new DomainException("Content has already been published.");
            }
        }

        private void VerifyNotSubmitted()
        {
            if (IsSubmitted)
            {
                throw new DomainException("Content has already been submitted.");
            }
        }

        private void VerifyHighestRightsAuthor(ContentCommand command)
        {
            List<string> notAllowed = new List<string>
            {
                SquidexRoles.Administrator,
                SquidexRoles.AppOwner,
                SquidexRoles.AppDeveloper,
                SquidexRoles.AppEditor
            };
            if (!command.Roles.Contains(SquidexRoles.AppAuthor) || command.Roles.Any(x => notAllowed.Contains(x)))
            {
                throw new DomainException("User is not an author.");
            }
        }

        private void VerifyIsEditor(ContentCommand command)
        {
            if (!command.Roles.Contains(SquidexRoles.AppEditor))
            {
                throw new DomainException("User does not have permission.");
            }
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }
    }
}
