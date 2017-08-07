// ==========================================================================
//  StringField.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using Squidex.Domain.Apps.Core.Schemas.Validators;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public sealed class TagField : Field<TagFieldProperties>
    {
        public TagField(long id, string name, Partitioning partitioning, TagFieldProperties properties1)
            : base(id, name, partitioning, properties1)
        {
        }

        public override object ConvertValue(JToken value)
        {
            return value.ToString().ToLower().Split(',').ToArray();
        }

        protected override IEdmTypeReference CreateEdmType()
        {
            // note: unsure if there is a way to represent collection/array of strings
            return EdmCoreModel.Instance.GetPrimitive(EdmPrimitiveTypeKind.String, !Properties.IsRequired);
        }

        protected override IEnumerable<IValidator> CreateValidators()
        {
            yield break;
        }

        protected override void PrepareJsonSchema(JsonProperty jsonProperty, Func<string, JsonSchema4, JsonSchema4> schemaResolver)
        {
            jsonProperty.Type = JsonObjectType.Array;
        }
    }
}
