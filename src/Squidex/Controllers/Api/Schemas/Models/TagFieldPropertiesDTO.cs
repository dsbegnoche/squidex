using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NJsonSchema.Annotations;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Squidex.Controllers.Api.Schemas.Models
{
    [JsonSchema("Tag")]
    public sealed class TagFieldPropertiesDTO : FieldPropertiesDto
    {
        /// <summary>
        /// The default value for the field value.
        /// </summary>
        public bool? DefaultValue { get; set; }

        /// <summary>
        /// The editor that is used to manage this field.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public TagFieldEditor Editor { get; set; }

        public override FieldProperties ToProperties()
        {
            var result = SimpleMapper.Map(this, new TagFieldProperties());
            return result;
        }
    }
}