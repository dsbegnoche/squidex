// ==========================================================================
//  ImageSharpAssetThumbnailGenerator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Squidex.Infrastructure.Assets.ImageSharp
{
    public sealed class ImageSharpAssetThumbnailGenerator : IAssetThumbnailGenerator
    {
        public ImageSharpAssetThumbnailGenerator()
        {
            Configuration.Default.AddImageFormat(ImageFormats.Jpeg);
            Configuration.Default.AddImageFormat(ImageFormats.Png);
        }

        public Task CreateThumbnailAsync(Stream source, Stream destination, int? width, int? height, string mode)
        {
            return Task.Run(() =>
            {
                if (width == null && height == null)
                {
                    source.CopyTo(destination);

                    return;
                }

                if (!Enum.TryParse<ResizeMode>(mode, true, out var resizeMode))
                {
                    resizeMode = ResizeMode.Max;
                }

                var w = width ?? int.MaxValue;
                var h = height ?? int.MaxValue;

                using (var sourceImage = Image.Load(source, out var format))
                {
                    if (w >= sourceImage.Width && h >= sourceImage.Height && resizeMode == ResizeMode.Crop)
                    {
                        resizeMode = ResizeMode.BoxPad;
                    }

                    var options = new ResizeOptions { Size = new Size(w, h), Mode = resizeMode };

                    sourceImage.Mutate(x => x.Resize(options));
                    sourceImage.Save(destination, format);
                }

                // TODO: Revisit with future release of ImageSharp
                GC.Collect();
            });
        }

        /// <summary> Get information about an image from a source stream</summary>
        /// <param name="source"> The file stream to get image info about</param>
        // note: this is not related to Thumbnail generation it seems just like a
        //       utility function used for getting information on asset creatiuon.
        public Task<ImageInfo> GetImageInfoAsync(Stream source)
        {
            return Task.Run(() =>
            {
                try
                {
                    using (var image = Image.Load(source))
                    {
                        return new ImageInfo(image.Width, image.Height);
                    }
                }
                catch
                {
                    return null;
                }
            });
        }
    }
}
