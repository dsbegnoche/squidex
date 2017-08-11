// ==========================================================================
//  StringFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure;

// ReSharper disable ObjectCreationAsStatement

namespace Squidex.Domain.Apps.Core.Schemas
{
    [TypeName("TagField")]
    public sealed class TagFieldProperties : FieldProperties
    {
        public override JToken GetDefaultValue()
        {
            return DefaultValue;
        }

        public TagFieldEditor Editor { get; set; } = TagFieldEditor.Input;

        public string DefaultValue { get; set; } = "";

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
