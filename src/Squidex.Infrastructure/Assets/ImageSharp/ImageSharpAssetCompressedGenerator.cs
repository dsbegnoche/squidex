// ==========================================================================
//  ImageSharpAssetCompressedGenerator.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ImageSharp;
using ImageSharp.Formats;
using SixLabors.Primitives;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetCompressedGenerator : IAssetCompressedGenerator
    {
        private IAssetThumbnailGenerator assetThumbnailGenerator;

        public ImageSharpAssetCompressedGenerator(IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Configuration.Default.AddImageFormat(ImageFormats.Jpeg);
            Configuration.Default.AddImageFormat(ImageFormats.Png);

            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        // x1/y1 = x2/y2
        // returns y2
        private int Ratio(decimal x1, decimal y1, decimal x2) => (int)(x2 / (x1 / y1));

        private async Task ResizeIfBigAsync(Stream source, Stream destination, Image<Rgba32> sourceImage)
        {
            int maxBorder = 2600;

            source.Position = 0;
            var width = sourceImage.Width;
            var height = sourceImage.Height;

            if (width > height && width > maxBorder)
            {
                await assetThumbnailGenerator.CreateThumbnailAsync(source, destination, maxBorder, Ratio(width, height, maxBorder), "Crop");
            }
            else if (height > width && height > maxBorder)
            {
                await assetThumbnailGenerator.CreateThumbnailAsync(source, destination, Ratio(height, width, maxBorder), maxBorder, "Crop");
            }
            else if (width > maxBorder && height > maxBorder)
            {
                await assetThumbnailGenerator.CreateThumbnailAsync(source, destination, maxBorder, maxBorder, "Crop");
            }
            else
            {
                await source.CopyToAsync(destination);
            }
        }

        public async Task CreateCompressedAsync(Stream source, Stream destination) =>
            await Task.Run(async () =>
            {
                source.Position = 0;
                Stream input = GetTempStream();
                await ResizeIfBigAsync(source, input, Image.Load((FileStream)source));

                var encoderMap = new Dictionary<string, Func<IImageEncoder>>
                {
                    // ref: https://www.freeformatter.com/mime-types-list.html
                    { "image/jpeg", () => new JpegEncoder() { Quality = 75 } },
                    { "image/png", () => new PngEncoder() { CompressionLevel = 8 } },

                    // to consider in the future:
                    // { "image/bmp", () => new BmpEncoder() },
                    // { "image/gif", () => new GifEncoder() },
                };

                input.Position = 0;
                using (var inputImage = Image.Load(input, out var format))
                {
                    if (encoderMap.ContainsKey(format.DefaultMimeType))
                    {
                        inputImage.Save(destination, encoderMap[format.DefaultMimeType]());
                    }
                    else
                    {
                        input.Position = 0;
                        input.CopyTo(destination);
                    }
                }
            });

        private static FileStream GetTempStream()
        {
            var tempFileName = Path.GetTempFileName();

            return new FileStream(tempFileName,
                FileMode.Create,
                FileAccess.ReadWrite,
                FileShare.Delete, 1024 * 16,
                FileOptions.Asynchronous |
                FileOptions.DeleteOnClose |
                FileOptions.SequentialScan);
        }
    }
}
