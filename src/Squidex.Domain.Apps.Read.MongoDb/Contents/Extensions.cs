// ==========================================================================
//  Extensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
// CivicPlus - Functionality moved to Squidex.Domain.Apps.Read.Contents\Extensions.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Domain.Apps.Read.MongoDb.Contents
{
    public static class Extensions
    {
        public static BsonDocument ToBsonDocument(this IdContentData data, JsonSerializer jsonSerializer)
        {
            return (BsonDocument)JToken.FromObject(data, jsonSerializer).ToBson();
        }

        public static NamedContentData ToData(this BsonDocument document, Schema schema, List<Guid> deletedIds, JsonSerializer jsonSerializer)
        {
            return document
                .ToJson()
                .ToObject<IdContentData>(jsonSerializer)
                .ToCleanedReferences(schema, new HashSet<Guid>(deletedIds ?? new List<Guid>()))
                .ToNameModel(schema, true);
        }
    }
}
