// ==========================================================================
//  PartialElasticContentEntity.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Contents;
using Newtonsoft.Json;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Read.Elastic
{
    public class PartialElasticContentEntity
    {
        [JsonRequired]
        [JsonProperty("mt")]
        [JsonConverter(typeof(InstantConverter))]
        public Instant LastModified { get; set; }

        [JsonRequired]
        [JsonProperty("dt")]
        public string DataText { get; set; }

        [JsonRequired]
        [JsonProperty("vs")]
        public long Version { get; set; }

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
