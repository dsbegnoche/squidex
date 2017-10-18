// ==========================================================================
//  ElasticContentEntity.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Domain.Apps.Core.Contents;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Read.Elastic
{
    public sealed class ElasticContentEntity : PartialElasticContentEntity, IContentEntity, IElasticEntity
    {
        public Guid Id { get; set; }

        [JsonProperty("st")]
        [JsonRequired]
        public Status Status { get; set; }

        [JsonRequired]
        [JsonProperty("ct")]
        [JsonConverter(typeof(InstantConverter))]
        public Instant Created { get; set; }

        [JsonRequired]
        [JsonProperty("ai")]
        public Guid AppId { get; set; }

        [JsonRequired]
        [JsonProperty("si")]
        public Guid SchemaId { get; set; }

        [JsonRequired]
        [JsonProperty("cb")]
        [JsonConverter(typeof(RefTokenConverter))]
        public RefToken CreatedBy { get; set; }
    }
}
