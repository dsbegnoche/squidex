// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("MultiField")]
    public sealed class MultiFieldProperties : FieldProperties
    {
        private ImmutableList<string> allowedValues;

        public MultiFieldProperties ()
        {
        }

        public MultiFieldProperties (MultiFieldProperties properties)
        {
            AllowedValues = properties.allowedValues;
            Label = properties.Label;
            IsRequired = properties.IsRequired;
        }

        public override JToken GetDefaultValue() => DefaultValue;

        public MultiFieldEditor Editor { get; set; } = MultiFieldEditor.Multi;

        public string DefaultValue { get; set; } = string.Empty;

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

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            yield break;
            /* having this validation here means can't initially add field to schema.
             * string field gets around this by using Input as default which doesn't lean on AllowedValues.
            if ((AllowedValues == null || AllowedValues.Count == 0))
            {
                yield return new ValidationError("Multi option needs allowed values", nameof(AllowedValues));
            }
            */
        }
    }
}
