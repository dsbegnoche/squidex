// ==========================================================================
//  AssetFile.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;

namespace Squidex.Infrastructure.Assets
{
    public sealed class AssetFile
    {
        private readonly Func<Stream> openAction;

        public string FileName { get; }

        public string MimeType { get; }

        public long FileSize { get; }

        public string BriefDescription { get; }

        public string[] Tags { get; }

        public AssetConfig AssetConfig { get; }

        public long MaxAssetRepoSize { get; }

        public long CurrentAssetRepoSize { get; }

        public AssetFile(string fileName, string mimeType, long fileSize,
                         Func<Stream> openAction, string briefDescription,
                         string[] tags,
                         AssetConfig assetConfig = null, long? maxAssetRepoSize = null, long? currentAssetRepoSize = null)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNullOrEmpty(mimeType, nameof(mimeType));
            Guard.NotNull(openAction, nameof(openAction));
            Guard.GreaterEquals(fileSize, 0, nameof(fileSize));

            FileName = fileName;
            FileSize = fileSize;
            BriefDescription = briefDescription;
            Tags = tags;

            MimeType = mimeType;

            this.openAction = openAction;

            // default values for testing purposes
            AssetConfig = assetConfig ?? new AssetConfig() { MaxSize = 1024 * 1024 * 50 };
            MaxAssetRepoSize = maxAssetRepoSize ?? AssetConfig.MaxSize * 1000;
            CurrentAssetRepoSize = currentAssetRepoSize ?? AssetConfig.MaxSize * 1;
        }

        public Stream OpenRead()
        {
            return openAction();
        }
    }
}
