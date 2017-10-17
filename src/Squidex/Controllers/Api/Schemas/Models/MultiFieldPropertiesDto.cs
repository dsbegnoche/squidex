// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Immutable;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonSchema("Multi")]
    public sealed class MultiFieldPropertiesDto : FieldPropertiesDto
    {
        /// <summary> The allowed values </summary>
        public string[] AllowedValues { get; set; }

        /// <summary> The default value for the field value. </summary>
        public string[] DefaultValue { get; set; }

        /// <summary> The editor that is used to manage this field.  </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public MultiFieldEditor Editor { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new MultiFieldProperties());

            if (AllowedValues != null)
            {
                result.AllowedValues = ImmutableList.Create(AllowedValues);
            }

            if (DefaultValue != null)
            {
                result.DefaultValues = ImmutableList.Create(DefaultValue);
            }

            return result;
        }
    }
}