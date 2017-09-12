// ==========================================================================
//  MongoUsage.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class MongoContentUsage
    {
        [BsonRequired]
        [BsonElement]
        [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
        public DateTime AccessDate { get; set; }

        [BsonRequired]
        [BsonElement]
        public Guid ContentId { get; set; }
    }
}
