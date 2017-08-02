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
            return "";
        }

        protected override IEnumerable<ValidationError> ValidateCore()
        {
            yield break;
        }
    }
}
