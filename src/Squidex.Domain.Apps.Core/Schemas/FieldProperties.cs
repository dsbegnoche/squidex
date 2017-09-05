// ==========================================================================
//  FieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;
using Squidex.Domain.Apps.Core.Schemas.Json;

namespace Squidex.Domain.Apps.Core.Schemas
{
    [JsonConverter(typeof(JsonInheritanceConverter), "fieldType")]
    [KnownType(typeof(AssetsFieldProperties))]
    [KnownType(typeof(BooleanFieldProperties))]
    [KnownType(typeof(DateTimeFieldProperties))]
    [KnownType(typeof(GeolocationFieldProperties))]
    [KnownType(typeof(JsonFieldProperties))]
    [KnownType(typeof(NumberFieldProperties))]
    [KnownType(typeof(ReferencesFieldProperties))]
    [KnownType(typeof(StringFieldProperties))]
    [KnownType(typeof(TagFieldProperties))]
    public abstract class FieldProperties : NamedElementPropertiesBase, IValidatable
    {
        private bool isRequired;
        private bool isListField;
        private string placeholder;

        public bool IsRequired
        {
            get { return isRequired; }
            set
            {
                ThrowIfFrozen();

                isRequired = value;
            }
        }

        public bool IsListField
        {
            get { return isListField; }
            set
            {
                ThrowIfFrozen();

                isListField = value;
            }
        }

        public string Placeholder
        {
            get { return placeholder; }
            set
            {
                ThrowIfFrozen();

                placeholder = value;
            }
        }

        public abstract JToken GetDefaultValue();

        public void Validate(IList<ValidationError> errors)
        {
            foreach (var error in ValidateCore())
            {
                errors.Add(error);
            }
        }

        protected abstract IEnumerable<ValidationError> ValidateCore();
    }
}