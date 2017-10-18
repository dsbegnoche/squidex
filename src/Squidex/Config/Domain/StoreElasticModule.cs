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
using Microsoft.Extensions.Configuration;
using Nest;
using Squidex.Domain.Apps.Read.Elastic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

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

                builder.RegisterType<ElasticRepository>()
                    .WithParameters(new Parameter[] { new TypedParameter(typeof(string), prefix), ResolvedParameter.ForNamed<IElasticClient>(ElasticInstanceName) })
                    .As<IEventConsumer>()
                    .AsSelf()
                    .SingleInstance();
            }
        }
    }
}
