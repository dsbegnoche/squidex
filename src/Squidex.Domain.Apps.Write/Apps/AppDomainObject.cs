﻿// ==========================================================================
//  AppDomainObject.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Apps;
using Squidex.Domain.Apps.Write.Apps.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppDomainObject : DomainObjectBase
    {
        private static readonly Language DefaultLanguage = Language.EN;
        private readonly AppContributors contributors = new AppContributors();
        private readonly AppClients clients = new AppClients();
        private readonly AppPatterns patterns = new AppPatterns();
        private LanguagesConfig languagesConfig = LanguagesConfig.Empty;
        private string name;
        private string planId;
        private RefToken planOwner;
        private IOptions<MyUIOptions> uiOptions;

        public string Name
        {
            get { return name; }
        }

        public string PlanId
        {
            get { return planId; }
        }

        public int ContributorCount
        {
            get { return contributors.Count; }
        }

        public IReadOnlyDictionary<string, AppPattern> Patterns => patterns.Patterns;

        public AppDomainObject(IOptions<MyUIOptions> uiOptions, Guid id, int version)
            : base(id, version)
        {
            this.uiOptions = uiOptions;
        }

        protected void On(AppCreated @event)
        {
            name = @event.Name;
        }

        protected void On(AppContributorAssigned @event)
        {
            contributors.Assign(@event.ContributorId, @event.Permission);
        }

        protected void On(AppContributorRemoved @event)
        {
            contributors.Remove(@event.ContributorId);
        }

        protected void On(AppClientAttached @event)
        {
            clients.Add(@event.Id, @event.Secret);
        }

        protected void On(AppClientChanged @event)
        {
            clients.Change(@event.Id, @event.IsReader);
        }

        protected void On(AppClientRenamed @event)
        {
            clients.Rename(@event.Id, @event.Name);
        }

        protected void On(AppClientRevoked @event)
        {
            clients.Revoke(@event.Id);
        }

        protected void On(AppLanguageAdded @event)
        {
            languagesConfig = languagesConfig.Add(@event.Language);
        }

        protected void On(AppLanguageRemoved @event)
        {
            languagesConfig = languagesConfig.Remove(@event.Language);
        }

        protected void On(AppLanguageUpdated @event)
        {
            languagesConfig = languagesConfig.Update(@event.Language, @event.IsOptional, @event.IsMaster, @event.Fallback);
        }

        protected void On(AppPlanChanged @event)
        {
            planId = @event.PlanId;

            planOwner = string.IsNullOrWhiteSpace(planId) ? null : @event.Actor;
        }

        protected void On(AppPatternAdded @event)
        {
            patterns.Add(@event.Name, @event.Pattern, @event.DefaultMessage);
        }

        protected void On(AppPatternDeleted @event)
        {
            patterns.Remove(@event.Name);
        }

        protected void On(AppPatternUpdated @event)
        {
            patterns.Update(@event.OriginalName, @event.Name, @event.Pattern, @event.DefaultMessage);
        }

        protected override void DispatchEvent(Envelope<IEvent> @event)
        {
            this.DispatchAction(@event.Payload);
        }

        public AppDomainObject Delete(DeleteApp command)
        {
            RaiseEvent(SimpleMapper.Map(command, new AppDeleted()));
            return this;
        }

        public AppDomainObject Create(CreateApp command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot create app");

            ThrowIfCreated();

            var appId = new NamedId<Guid>(command.AppId, command.Name);

            RaiseEvent(SimpleMapper.Map(command, new AppCreated { AppId = appId }));

            RaiseEvent(SimpleMapper.Map(command, CreateInitialOwner(appId, command)));
            RaiseEvent(SimpleMapper.Map(command, CreateInitialLanguage(appId)));
            CreateInitialPatterns(command);

            return this;
        }

        public AppDomainObject UpdateClient(UpdateClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update client");

            ThrowIfNotCreated();

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientRenamed()));
            }

            if (command.IsReader.HasValue)
            {
                RaiseEvent(SimpleMapper.Map(command, new AppClientChanged { IsReader = command.IsReader.Value }));
            }

            return this;
        }

        public AppDomainObject AssignContributor(AssignContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot assign contributor");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorAssigned()));

            return this;
        }

        public AppDomainObject RemoveContributor(RemoveContributor command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove contributor");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppContributorRemoved()));

            return this;
        }

        public AppDomainObject AttachClient(AttachClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot attach client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientAttached()));

            return this;
        }

        public AppDomainObject RevokeClient(RevokeClient command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot revoke client");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppClientRevoked()));

            return this;
        }

        public AppDomainObject AddLanguage(AddLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot add language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageAdded()));

            return this;
        }

        public AppDomainObject RemoveLanguage(RemoveLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot remove language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageRemoved()));

            return this;
        }

        public AppDomainObject UpdateLanguage(UpdateLanguage command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update language");

            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppLanguageUpdated()));

            return this;
        }

        public AppDomainObject ChangePlan(ChangePlan command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot change plan");

            ThrowIfNotCreated();
            ThrowIfOtherUser(command);

            RaiseEvent(SimpleMapper.Map(command, new AppPlanChanged()));

            return this;
        }

        public AppDomainObject AddPattern(AddPattern command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot add pattern");
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPatternAdded()));

            return this;
        }

        public AppDomainObject DeletePattern(DeletePattern command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot delete pattern");
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPatternDeleted()));

            return this;
        }

        public AppDomainObject UpdatePattern(UpdatePattern command)
        {
            Guard.Valid(command, nameof(command), () => "Cannot update pattern");
            ThrowIfNotCreated();

            RaiseEvent(SimpleMapper.Map(command, new AppPatternUpdated()));

            return this;
        }

        private void RaiseEvent(AppEvent @event)
        {
            if (@event.AppId == null)
            {
                @event.AppId = new NamedId<Guid>(Id, name);
            }

            RaiseEvent(Envelope.Create(@event));
        }

        private static AppLanguageAdded CreateInitialLanguage(NamedId<Guid> id)
        {
            return new AppLanguageAdded { AppId = id, Language = DefaultLanguage };
        }

        private static AppContributorAssigned CreateInitialOwner(NamedId<Guid> id, SquidexCommand command)
        {
            return new AppContributorAssigned { AppId = id, ContributorId = command.Actor.Identifier, Permission = PermissionLevel.Owner };
        }

        private void CreateInitialPatterns(SquidexCommand command)
        {
            foreach (var option in uiOptions.Value.RegexSuggestions)
            {
                RaiseEvent(SimpleMapper.Map(command, new AppPatternAdded
                {
                    Name = option.Name,
                    Pattern = option.Pattern,
                    DefaultMessage = option.DefaultMessage
                }));
            }
        }

        private void ThrowIfOtherUser(ChangePlan command)
        {
            if (!string.IsNullOrWhiteSpace(command.PlanId) && planOwner != null && !planOwner.Equals(command.Actor))
            {
                throw new ValidationException("Plan can only be changed from current user.");
            }

            if (string.Equals(command.PlanId, planId, StringComparison.OrdinalIgnoreCase))
            {
                throw new ValidationException("App has already this plan.");
            }
        }

        private void ThrowIfNotCreated()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has not been created.");
            }
        }

        private void ThrowIfCreated()
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("App has already been created.");
            }
        }
    }
}
