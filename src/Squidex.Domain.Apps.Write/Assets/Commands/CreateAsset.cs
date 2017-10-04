﻿// ==========================================================================
//  CreateAsset.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Infrastructure.Assets;

namespace Squidex.Domain.Apps.Write.Assets.Commands
{
    public sealed class CreateAsset : AssetAggregateCommand
    {
        public AssetFile File { get; set; }

        public ImageInfo ImageInfo { get; set; }

        public CompressedInfo CompressedImageInfo { get; set; }

        public CreateAsset()
        {
            AssetId = Guid.NewGuid();
        }
    }
}
