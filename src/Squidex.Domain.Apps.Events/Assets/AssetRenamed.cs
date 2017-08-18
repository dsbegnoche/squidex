﻿// ==========================================================================
//  AssetRenamed.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Events.Assets
{
    [TypeName("AssetRenamedEvent")]
    public class AssetRenamed : AssetEvent
    {
        public string FileName { get; set; }

        public string BriefDescription { get; set; }

        public string[] Tags { get; set; }
    }
}
