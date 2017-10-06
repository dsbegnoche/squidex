// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("MultiField")]
    public sealed class MultiFieldProperties : FieldProperties
    {
        public override JToken GetDefaultValue()
        {
            return DefaultValue;
        }

        public MultiFieldEditor Editor { get; set; } = MultiFieldEditor.MultiInput;

        public string DefaultValue { get; set; } = string.Empty;

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            // you have to have some kind of return value, and doing the
            // condition like this evades the unreachable code warning.
            var condition = false;
            if (condition)
            {
                yield return new ValidationError("This should never be called.");
            }
        }
    }
}
