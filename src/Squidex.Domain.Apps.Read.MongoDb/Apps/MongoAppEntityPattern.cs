/*
 * CivicPlus implementation of Squidex Headless CMS
 */

using MongoDB.Bson.Serialization.Attributes;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Read.MongoDb.Apps
{
    public sealed class MongoAppEntityPattern : IAppPatternEntity
    {
        [BsonRequired]
        [BsonElement]
        public string Name { get; set; }

        [BsonRequired]
        [BsonElement]
        public string Pattern { get; set; }

        [BsonElement]
        public string DefaultMessage { get; set; }
    }
}
