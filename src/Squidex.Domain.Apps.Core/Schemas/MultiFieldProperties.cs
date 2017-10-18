// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("MultiField")]
    public sealed class MultiFieldProperties : FieldProperties
    {
        private ImmutableList<string> allowedValues;
        private ImmutableList<string> defaultValues;

        public MultiFieldProperties ()
        {
        }

        public MultiFieldProperties (MultiFieldProperties properties)
        {
            AllowedValues = properties.allowedValues;
            Label = properties.Label;
            IsRequired = properties.IsRequired;
            DefaultValue = properties.DefaultValue;
        }

        public override JToken GetDefaultValue() => new JArray();

        public override T Accept<T>(IFieldPropertiesVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        public MultiFieldEditor Editor { get; set; } = MultiFieldEditor.Multi;

        public ImmutableList<string> DefaultValue
        {
            get
            {
                return defaultValues;
            }
            set
            {
                ThrowIfFrozen();

                defaultValues = value;
            }
        }

        public ImmutableList<string> AllowedValues
        {
            get
            {
                return allowedValues;
            }
            set
            {
                ThrowIfFrozen();

                allowedValues = value;
            }
        }
    }
}
