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

        public AssetFile(string fileName, string mimeType, long fileSize, Func<Stream> openAction, string briefDescription)
        {
            Guard.NotNullOrEmpty(fileName, nameof(fileName));
            Guard.NotNullOrEmpty(mimeType, nameof(mimeType));
            Guard.NotNull(openAction, nameof(openAction));
            Guard.GreaterEquals(fileSize, 0, nameof(fileSize));

            FileName = fileName;
            FileSize = fileSize;
            BriefDescription = briefDescription;

            MimeType = mimeType;

            this.openAction = openAction;
        }

        public Stream OpenRead()
        {
            return openAction();
        }
    }
}
