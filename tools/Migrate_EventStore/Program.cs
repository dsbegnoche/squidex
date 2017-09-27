// ==========================================================================
//  Program.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Exceptions;
using EventStore.ClientAPI.Projections;
using MongoDB.Bson;
using MongoDB.Driver;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Events;

namespace Migrate_EventStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Migrate EventStore");

            var mongoClient = new MongoClient(GetMongoConnectionValue());
            var mongoDatabase = mongoClient.GetDatabase(GetMongoDatabaseValue());
            var eventStoreConnection = EventStoreConnection.Create(GetEventStoreConnectionValue());
            var prefix = GetEventStorePrefixValue();
            var projection = "localhost";

            var mongoEventStore = new MongoEventStore(mongoDatabase, new DefaultEventNotifier(new InMemoryPubSub()));
            var getEventStore = new GetEventStore(eventStoreConnection, prefix, projection);
            getEventStore.Connect();
            var collection = mongoDatabase.GetCollection<BsonDocument>("Events");

            var query =
                collection.Find(new BsonDocument())
                    .Project<BsonDocument>(
                        Builders<BsonDocument>.Projection.Include(Field("EventStream")))
                    .ToList().Select(x => x["EventStream"]).Distinct();
            Console.WriteLine($"Stream Count: {query.Count()}");

            List<string> filters = new List<string>
            {
                "^app-",
                "^(schema-)|(apps-)",
                "^content-",
                "^asset-",
                "^(content-)|(app-)|(asset-)",
                ".*",
                "^schema-",
                "(^webhook-)|(^schema-)"
            };

            foreach (string filter in filters)
            {
                CreateProjectionAsync(filter, eventStoreConnection, prefix, projection).Wait();
            }

            foreach (var stream in query)
            {
                var events = mongoEventStore.GetEventsAsync(stream.AsString).Result;
                Console.WriteLine($"{stream.AsString} - {events.Count}");
                getEventStore.AppendEventsAsync(Guid.NewGuid(), stream.AsString, events.Select(x => x.Data).ToList());
            }

            Console.WriteLine("DONE - ENTER to continue.");
            Console.ReadLine();
        }

        private static StringFieldDefinition<BsonDocument, T> Field<T>(string fieldName)
        {
            return new StringFieldDefinition<BsonDocument, T>(fieldName);
        }

        private static StringFieldDefinition<BsonDocument> Field(string fieldName)
        {
            return new StringFieldDefinition<BsonDocument>(fieldName);
        }

        private static string GetMongoConnectionValue()
        {
            Console.Write("Mongo Connection (ENTER for 'mongodb://localhost'): ");

            var mongoConnection = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(mongoConnection))
            {
                mongoConnection = "mongodb://localhost";
            }

            return mongoConnection;
        }

        private static string GetMongoDatabaseValue()
        {
            Console.Write("Mongo Database (ENTER for 'Squidex'): ");

            var mongoDatabase = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(mongoDatabase))
            {
                mongoDatabase = "Squidex";
            }

            return mongoDatabase;
        }

        private static string GetEventStoreConnectionValue()
        {
            Console.Write("GetEventStore Connection (ENTER for 'tcp://admin:changeit@localhost:1113'): ");
            var eventConnection = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(eventConnection))
            {
                eventConnection = "tcp://admin:changeit@localhost:1113";
            }

            return $"ConnectTo={eventConnection}; HeartBeatTimeout=500; MaxReconnections=-1";
        }

        private static string GetEventStorePrefixValue()
        {
            Console.Write("GetEventStore Prefix (ENTER for 'squidex'): ");

            var projection = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(projection))
            {
                projection = "squidex";
            }

            return projection;
        }

        private static async Task CreateProjectionAsync(string streamFilter, IEventStoreConnection connection, string prefix, string projectionHost)
        {
            string streamName = streamFilter.Simplify();
                var projectsManager = await ConnectToProjections(connection, projectionHost);

                var projectionConfig =
                    $@"fromAll()
                        .when({{
                            $any: function (s, e) {{
                                if (e.streamId.indexOf('{prefix}') === 0 && /{streamFilter}/.test(e.streamId.substring({prefix.Length + 1}))) {{
                                    linkTo('{streamName}', e);
                                }}
                            }}
                        }});";

                try
                {
                    await projectsManager.CreateContinuousAsync(
                        $"${streamName}",
                        projectionConfig,
                        connection.Settings.DefaultUserCredentials);
                }
                catch (ProjectionCommandConflictException)
                {
                    // ignore
                }
        }

        private static async Task<ProjectionsManager> ConnectToProjections(IEventStoreConnection connection, string projectionHost)
        {
            var addressParts = projectionHost.Split(':');

            if (addressParts.Length < 2 || !int.TryParse(addressParts[1], out var port))
            {
                port = 2113;
            }

            var endpoints = await Dns.GetHostAddressesAsync(addressParts[0]);
            var endpoint = new IPEndPoint(endpoints.First(x => x.AddressFamily == AddressFamily.InterNetwork), port);

            var projectionsManager =
                new ProjectionsManager(
                    connection.Settings.Log,
                    endpoint,
                    connection.Settings.OperationTimeout);

            return projectionsManager;
        }
    }
}
