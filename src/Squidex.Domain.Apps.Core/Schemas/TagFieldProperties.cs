// ==========================================================================
//  StringFieldProperties.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
            var condition = false;
            if (condition)
            {
                yield return new ValidationError("nope");
            }
        }
    }
}
