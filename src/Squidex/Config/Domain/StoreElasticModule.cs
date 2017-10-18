// ==========================================================================
//  StoreMongoDbModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Autofac.Core;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Nest;
using Squidex.Domain.Apps.Read.Apps.Repositories;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Domain.Apps.Read.Contents.Repositories;
using Squidex.Domain.Apps.Read.History.Repositories;
using Squidex.Domain.Apps.Read.MongoDb.Apps;
using Squidex.Domain.Apps.Read.MongoDb.Assets;
using Squidex.Domain.Apps.Read.MongoDb.Contents;
using Squidex.Domain.Apps.Read.MongoDb.History;
using Squidex.Domain.Apps.Read.MongoDb.Schemas;
using Squidex.Domain.Apps.Read.MongoDb.Webhooks;
using Squidex.Domain.Apps.Read.Schemas.Repositories;
using Squidex.Domain.Apps.Read.Webhooks.Repositories;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Domain.Users.MongoDb.Infrastructure;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.UsageTracking;
using Squidex.Shared.Users;

namespace Squidex.Config.Domain
{
    public class StoreElasticModule : Module
    {
        private const string ElasticInstanceName = "ElasticClient";

        private IConfiguration Configuration { get; }

        public StoreElasticModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var elasticEndpoint = Configuration.GetValue<string>("elastic:endpoint");
            var username = Configuration.GetValue<string>("elastic:username");
            var password = Configuration.GetValue<string>("elastic:password");
            var prefix = Configuration.GetValue<string>("elastic:indexPrefix");

            if (Configuration.GetValue<bool>("elastic:enabled"))
            {
                ConnectionSettings settings = new ConnectionSettings(new Uri(elasticEndpoint));
                settings.BasicAuthentication(username, password);

                builder.Register(c => Singletons<IElasticClient>.GetOrAdd("elastic", s => new ElasticClient(settings)))
                    .Named<IElasticClient>(ElasticInstanceName)
                    .SingleInstance();

                builder.RegisterType<ElasticContentRepository>()
                    .WithParameters(new Parameter[] { new TypedParameter(typeof(string), prefix), ResolvedParameter.ForNamed<IElasticClient>(ElasticInstanceName) })
                    .As<IEventConsumer>()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}
