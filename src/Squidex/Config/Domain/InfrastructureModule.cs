﻿// ==========================================================================
//  InfrastructureModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.Schemas.Json;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Actors;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Assets.ImageSharp;
using Squidex.Infrastructure.Caching;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Suggestions;
using Squidex.Infrastructure.Suggestions.Services;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Pipeline;

namespace Squidex.Config.Domain
{
    public sealed class InfrastructureModule : Module
    {
        private IConfiguration Configuration { get; }

        public InfrastructureModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (Configuration.GetValue<bool>("logging:human"))
            {
                builder.Register(c => new Func<IObjectWriter>(() => new JsonLogWriter(Formatting.Indented, true)))
                    .AsSelf()
                    .SingleInstance();
            }
            else
            {
                builder.Register(c => new Func<IObjectWriter>(() => new JsonLogWriter()))
                    .AsSelf()
                    .SingleInstance();
            }

            var loggingFile = Configuration.GetValue<string>("logging:file");

            if (!string.IsNullOrWhiteSpace(loggingFile))
            {
                builder.RegisterInstance(new FileChannel(loggingFile))
                    .As<ILogChannel>()
                    .As<IExternalSystem>()
                    .SingleInstance();
            }

            var imageSuggestionServiceType = Configuration.GetValue<string>("suggestionServices:images");
            var textSuggestionServiceType = Configuration.GetValue<string>("suggestionServices:text");

            if (string.IsNullOrWhiteSpace(imageSuggestionServiceType))
            {
                throw new ConfigurationException("Configure SuggestionServices type with 'suggestionServices:images'.");
            }

            if (string.IsNullOrWhiteSpace(textSuggestionServiceType))
            {
                throw new ConfigurationException("Configure SuggestionServices type with 'suggestionServices:text'.");
            }

            if (string.Equals(imageSuggestionServiceType, "Azure", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterType<AzureImageSuggestionService>()
                    .As<IImageSuggestionService>()
                    .AsSelf()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{imageSuggestionServiceType}' for 'suggestionServices:images', supported: Azure.");
            }

            if (string.Equals(textSuggestionServiceType, "Watson", StringComparison.OrdinalIgnoreCase))
            {
                builder.Register(c => new WatsonTextSuggestionService())
                    .As<ITextSuggesionService>()
                    .AsSelf()
                    .SingleInstance();
            }
            else if (string.Equals(textSuggestionServiceType, "Azure", StringComparison.OrdinalIgnoreCase))
            {
                builder.RegisterType<AzureTextSuggestionService>()
                    .As<ITextSuggesionService>()
                    .AsSelf()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{imageSuggestionServiceType}' for 'suggestionServices:images', supported: Azure.");
            }

            builder.Register(c =>
                new AssetSuggestions(
                    c.Resolve<IOptions<AuthenticationKeys>>(),
                    c.Resolve<IImageSuggestionService>(),
                    c.Resolve<ITextSuggesionService>()))
                .As<IAssetSuggestions>()
                .AsSelf()
                .SingleInstance();

            builder.Register(c => new ApplicationInfoLogAppender(GetType(), Guid.NewGuid()))
                .As<ILogAppender>()
                .SingleInstance();

            builder.RegisterType<ActionContextLogAppender>()
                .As<ILogAppender>()
                .SingleInstance();

            builder.RegisterType<TimestampLogAppender>()
                .As<ILogAppender>()
                .SingleInstance();

            builder.RegisterType<DebugLogChannel>()
                .As<ILogChannel>()
                .SingleInstance();

            builder.RegisterType<ConsoleLogChannel>()
                .As<ILogChannel>()
                .SingleInstance();

            builder.RegisterType<SemanticLog>()
                .As<ISemanticLog>()
                .SingleInstance();

            builder.Register(c => SystemClock.Instance)
                .As<IClock>()
                .SingleInstance();

            builder.RegisterType<BackgroundUsageTracker>()
                .As<IUsageTracker>()
                .SingleInstance();

            builder.RegisterType<ContentUsageTracker>()
                .As<IContentUsageTracker>()
                .SingleInstance();

            builder.RegisterType<HttpContextAccessor>()
                .As<IHttpContextAccessor>()
                .SingleInstance();

            builder.RegisterType<ActionContextAccessor>()
                .As<IActionContextAccessor>()
                .SingleInstance();

            builder.RegisterType<DefaultDomainObjectRepository>()
                .As<IDomainObjectRepository>()
                .SingleInstance();

            builder.RegisterType<DefaultDomainObjectFactory>()
                .As<IDomainObjectFactory>()
                .SingleInstance();

            builder.RegisterType<AggregateHandler>()
                .As<IAggregateHandler>()
                .SingleInstance();

            builder.RegisterType<InMemoryCommandBus>()
                .As<ICommandBus>()
                .SingleInstance();

            builder.RegisterType<DefaultEventNotifier>()
                .As<IEventNotifier>()
                .SingleInstance();

            builder.RegisterType<DefaultStreamNameResolver>()
                .As<IStreamNameResolver>()
                .SingleInstance();

            builder.RegisterType<ImageSharpAssetThumbnailGenerator>()
                .As<IAssetThumbnailGenerator>()
                .SingleInstance();

            builder.RegisterType<ImageSharpAssetCompressedGenerator>()
                .As<IAssetCompressedGenerator>()
                .SingleInstance();

            builder.Register(c => new InvalidatingMemoryCache(new MemoryCache(c.Resolve<IOptions<MemoryCacheOptions>>()), c.Resolve<IPubSub>()))
                .As<IMemoryCache>()
                .SingleInstance();

            builder.RegisterType<DefaultRemoteActorChannel>()
                .As<IRemoteActorChannel>()
                .SingleInstance();

            builder.RegisterType<RemoteActors>()
                .As<IActors>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<EventConsumerCleaner>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<EventDataFormatter>()
                .AsSelf()
                .SingleInstance();
        }
    }
}
