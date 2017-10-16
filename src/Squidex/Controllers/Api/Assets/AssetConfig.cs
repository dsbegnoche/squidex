// ==========================================================================
//  AssetConfig.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
// CivicPlus - Functionality moved to Squidex.Infrastructure\Assets\AssetConfig.cs

namespace Squidex.Controllers.Api.Assets
{
    public sealed class AssetConfig
    {
        public long MaxSize { get; set; } = 5 * 1024 * 1024;
    }
}
