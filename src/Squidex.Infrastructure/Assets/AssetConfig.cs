// ==========================================================================
//  AssetConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Squidex.Infrastructure.Assets
{
    public class AssetConfig
    {
        public long MaxSize { get; set; } = 5 * 1024 * 1024;
    }
}
