// ==========================================================================
//  ElasticContentEntity.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================
// 
using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Core.Contents;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Read.Elastic
{
    public sealed class ElasticContentEntity : IContentEntity, IElasticEntity
    {
        public Guid Id { get; set; }

        [JsonProperty("st")]
        [JsonRequired]
        public Status Status { get; set; }

        [JsonRequired]
        [JsonProperty("ct")]
        public Instant Created { get; set; }

        [JsonRequired]
        [JsonProperty("mt")]
        public Instant LastModified { get; set; }

        [JsonRequired]
        [JsonProperty("dt")]
        public string DataText { get; set; }

        [JsonRequired]
        [JsonProperty("vs")]
        public long Version { get; set; }

        [JsonRequired]
        [JsonProperty("ai")]
        public Guid AppId { get; set; }

        [JsonRequired]
        [JsonProperty("si")]
        public Guid SchemaId { get; set; }

        [JsonRequired]
        [JsonProperty("cb")]
        public RefToken CreatedBy { get; set; }

        [JsonRequired]
        [JsonProperty("mb")]
        public RefToken LastModifiedBy { get; set; }

        [JsonRequired]
        [JsonProperty("do")]
        public NamedContentData Data { get; set; }

        [JsonRequired]
        [JsonProperty("rf")]
        public List<Guid> ReferencedIds { get; set; }

        [JsonRequired]
        [JsonProperty("rd")]
        public List<Guid> ReferencedIdsDeleted { get; set; } = new List<Guid>();
    }
}
