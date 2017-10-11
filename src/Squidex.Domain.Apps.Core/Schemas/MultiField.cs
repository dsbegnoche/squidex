// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class MultiField : Field<MultiFieldProperties>
    {
        public MultiField(long id, string name, Partitioning partitioning, MultiFieldProperties properties1)
            : base(id, name, partitioning, properties1)
        {
        }

        public override object ConvertValue(JToken value)
        {
            return value;
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            // note: unsure if there is a way to represent collection/array of strings
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, !Properties.IsRequired);
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            if (Properties.AllowedValues != null)
            {
                yield return new AllowedValuesValidatorMultiple<string>(Properties.AllowedValues.ToArray());
            }
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.Array;
        }
    }
}
