// ==========================================================================
//  MyUIOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
// CivicPlus - Functionality moved to Squidex.Domain.Apps.Core\MyUIOptions.cs

using System.Collections.Generic;

namespace Squidex.Config
{
    public sealed class MyUIOptions
    {
        public Dictionary<string, string> RegexSuggestions { get; set; }
    }
}
