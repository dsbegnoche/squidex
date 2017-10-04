using System;
using System.Collections.Generic;
using System.Text;

namespace Squidex.Infrastructure.Assets
{
    public sealed class CompressedInfo
    {
        public int PixelWidth { get; }

        public int PixelHeight { get; }

        public long FileSize { get; }

        public CompressedInfo(int pixelWidth, int pixelHeight, long fileSize)
        {
            Guard.GreaterThan(pixelWidth, 0, nameof(pixelWidth));
            Guard.GreaterThan(pixelHeight, 0, nameof(pixelHeight));
            Guard.GreaterThan(pixelHeight, 0, nameof(fileSize));

            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            FileSize = fileSize;
        }
    }
}
