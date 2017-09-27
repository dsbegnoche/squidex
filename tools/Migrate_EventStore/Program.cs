// ==========================================================================
//  Program.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using EventStore.ClientAPI;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Migrate_EventStore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Migrate EventStore");

            var mongoClient = new MongoClient(GetMongoConnectionValue());
            var mongoDatabase = mongoClient.GetDatabase(GetMongoDatabaseValue());
            var eventStoreClient = EventStoreConnection.Create(GetEventStoreConnectionValue());
            var prefix = GetEventStorePrefixValue();
            var projection = "localhost";

            var collection = mongoDatabase.GetCollection<BsonDocument>("Events");

            var query =
                collection.Find(new BsonDocument())
                    .SortBy(x => x["Timestamp"])
                    .Project<BsonDocument>(
                        Builders<BsonDocument>.Projection.Include(Field("EventsOffset")))
                    .ToList();

            Console.Write("Migrate Events...");

            foreach (var eventCommit in query)
            {
                var eventsOffset = (int)eventCommit["EventsOffset"].AsInt64;

                var ts = new BsonTimestamp(eventsOffset + 10, 1);

                collection.UpdateOne(
                    Builders<BsonDocument>.Filter
                        .Eq(Field<string>("_id"), eventCommit["_id"].AsString),
                    Builders<BsonDocument>.Update
                        .Set(Field<BsonTimestamp>("Timestamp"), ts).Unset(Field("EventsOffset")));
            }

            Console.WriteLine("DONE");
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

            return eventConnection;
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
    }
}
