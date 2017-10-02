﻿// ==========================================================================
//  EventStoreModule.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Autofac;
using Autofac.Core;
using EventStore.ClientAPI;
using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Table;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.CQRS.Events.Actors;

namespace Squidex.Config.Domain
{
    public sealed class EventStoreModule : Module
    {
        private const string MongoClientRegistration = "EventStoreMongoClient";
        private const string MongoDatabaseRegistration = "EventStoreMongoDatabase";

        private IConfiguration Configuration { get; }

        public EventStoreModule(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var consumeEvents = Configuration.GetValue<bool>("eventStore:consume");

            if (consumeEvents)
            {
                builder.RegisterType<EventConsumerActor>()
                    .AsSelf()
                    .InstancePerDependency();
            }

            var eventStoreType = Configuration.GetValue<string>("eventStore:type");

            if (string.IsNullOrWhiteSpace(eventStoreType))
            {
                throw new ConfigurationException("Configure EventStore type with 'eventStore:type'.");
            }

            if (string.Equals(eventStoreType, "MongoDb", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = Configuration.GetValue<string>("eventStore:mongoDb:configuration");

                if (string.IsNullOrWhiteSpace(configuration))
                {
                    throw new ConfigurationException("Configure EventStore MongoDb configuration with 'eventStore:mongoDb:configuration'.");
                }

                var database = Configuration.GetValue<string>("eventStore:mongoDb:database");

                if (string.IsNullOrWhiteSpace(database))
                {
                    throw new ConfigurationException("Configure EventStore MongoDb Database name with 'eventStore:mongoDb:database'.");
                }

                builder.Register(c => Singletons<IMongoClient>.GetOrAdd(configuration, s => new MongoClient(s)))
                    .Named<IMongoClient>(MongoClientRegistration)
                    .SingleInstance();

                builder.Register(c => c.ResolveNamed<IMongoClient>(MongoClientRegistration).GetDatabase(database))
                    .Named<IMongoDatabase>(MongoDatabaseRegistration)
                    .SingleInstance();

                builder.RegisterType<MongoEventStore>()
                    .WithParameter(ResolvedParameter.ForNamed<IMongoDatabase>(MongoDatabaseRegistration))
                    .As<IExternalSystem>()
                    .As<IEventStore>()
                    .SingleInstance();
            }
            else if (string.Equals(eventStoreType, "GetEventStore", StringComparison.OrdinalIgnoreCase))
            {
                var configuration = Configuration.GetValue<string>("eventStore:getEventStore:configuration");

                if (string.IsNullOrWhiteSpace(configuration))
                {
                    throw new ConfigurationException("Configure GetEventStore EventStore configuration with 'eventStore:getEventStore:configuration'.");
                }

                var projectionHost = Configuration.GetValue<string>("eventStore:getEventStore:projectionHost");

                if (string.IsNullOrWhiteSpace(projectionHost))
                {
                    throw new ConfigurationException("Configure GetEventStore EventStore projection host with 'eventStore:getEventStore:projectionHost'.");
                }

                var prefix = Configuration.GetValue<string>("eventStore:getEventStore:prefix");

                var connection = EventStoreConnection.Create(configuration);

                builder.Register(c => new GetEventStore(connection, prefix, projectionHost))
                    .As<IExternalSystem>()
                    .As<IEventStore>()
                    .SingleInstance();
            }
            else if (string.Equals(eventStoreType, "AzureTableEventStore", StringComparison.OrdinalIgnoreCase))
            {
                var accountName = Configuration.GetValue<string>("eventStore:azureTableStorage:accountName");
                var keyValue = Configuration.GetValue<string>("eventStore:azureTableStorage:keyValue");
                var uri = Configuration.GetValue<string>("eventStore:azureTableStorage:uri");

                var credentials = new StorageCredentials(accountName, keyValue);

                var tableAddress = new Uri(uri);

                if (string.IsNullOrWhiteSpace(credentials.SASToken))
                {
                    throw new ConfigurationException("Configure Azure Table Storage EventStore configuration with 'eventStore:azureTableStorage:key'.");
                }

                if (string.IsNullOrWhiteSpace(credentials.AccountName))
                {
                    throw new ConfigurationException("Configure Azure Table Storage EventStore configuration with 'eventStore:azureTableStorage:accountName'.");
                }

                if (string.IsNullOrWhiteSpace(tableAddress.AbsolutePath))
                {
                    throw new ConfigurationException("Configure Azure Table Storage EventStore configuration with 'eventStore:azureTableStorage:uri'.");
                }

                var prefix = Configuration.GetValue<string>("eventStore:getEventStore:prefix");

                var cloudStorageAccount =
                    CloudStorageAccount.Parse(
                        $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={keyValue}");

                var tableClient = cloudStorageAccount.CreateCloudTableClient();
                var eventStoreTable = tableClient.GetTableReference("EventStore");

                builder.Register(c => new TableStorageEventStore(cloudStorageAccount, tableClient, eventStoreTable, prefix))
                    .As<IExternalSystem>()
                    .As<IEventStore>()
                    .SingleInstance();
            }
            else
            {
                throw new ConfigurationException($"Unsupported value '{eventStoreType}' for 'eventStore:type', supported: MongoDb, GetEventStore.");
            }
        }
    }
}
