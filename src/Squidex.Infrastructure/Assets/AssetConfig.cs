// ==========================================================================
//  AssetConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Assets
{
    public class AssetConfig
    {
        // 1 * 1024 ^ 3 = 1 GB
        // 500 * 1024 ^ 2 = 500 MB
        public long MaxSize { get; set; } = (long)Math.Pow(1024, 2) * 500;
    }
}
