// ==========================================================================
//  AssetContentController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Domain.Apps.Read.Assets.Repositories;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

#pragma warning disable 1573

namespace Squidex.Controllers.Api.Assets
{
    /// <summary>
    /// Uploads and retrieves assets.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(Assets))]
    public sealed class AssetContentController : ControllerBase
    {
        private readonly IAssetStore assetStorage;
        private readonly IAssetRepository assetRepository;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetContentController(
            ICommandBus commandBus,
            IAssetStore assetStorage,
            IAssetRepository assetRepository,
            IAssetThumbnailGenerator assetThumbnailGenerator)
            : base(commandBus)
        {
            this.assetStorage = assetStorage;
            this.assetRepository = assetRepository;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }

        /// <summary>
        /// Get the asset content.
        /// </summary>
        /// <param name="app">The name of the app.</param>
        /// <param name="id">The id of the asset.</param>
        /// <param name="version">The optional version of the asset.</param>
        /// <param name="width">The target width of the asset, if it is an image.</param>
        /// <param name="height">The target height of the asset, if it is an image.</param>
        /// <param name="mode">
        /// The resize mode of the asset if it is an image: "Crop", "Full", "Compressed"(default).
        /// if "crop", width and height are used.
        /// </param>
        /// <returns>
        /// 200 => Asset found and content or (resize) image returned.
        /// 404 => Asset or app not found.
        /// </returns>
        [HttpGet]
        [Route("assets/{id}/")]
        [ProducesResponseType(200)]
        public async Task<IActionResult> GetAssetContent(string app, Guid id, [FromQuery] int version = -1, [FromQuery] int? width = null, [FromQuery] int? height = null, [FromQuery] string mode = null)
        {
            var asset = await assetRepository.FindAssetAsync(id);

            if (asset == null || asset.FileVersion < version || width == 0 || height == 0)
            {
                return NotFound();
            }

            var assetId = asset.Id.ToString();

            return new FileCallbackResult(asset.MimeType, asset.FileName, async bodyStream =>
            {
                if (asset.IsImage)
                {
                    mode = mode?.ToLower() ?? "compressed";
                    string assetSuffix = null;

                    switch (mode)
                    {
                        case "crop":
                            if (!(width.HasValue || height.HasValue))
                            {
                                throw new InvalidOperationException("crop mode requires height and/or width to crop to");
                            }
                            assetSuffix = $"{width}_{height}_Crop";
                            break;

                        case "compressed":
                            assetSuffix = "Compressed";
                            break;

                        case "full":
                            // null assetSuffix is full.
                            break;
                    }

                    try
                    {
                        await assetStorage.DownloadAsync(assetId, asset.FileVersion, assetSuffix, bodyStream);
                    }
                    catch (AssetNotFoundException)
                    {
                        if (mode == "Compressed")
                        {
                            // default to full
                            await assetStorage.DownloadAsync(assetId, asset.FileVersion, null, bodyStream);
                            return;
                        }

                        // generate thumbnail from full
                        using (var sourceStream = AssetUtil.GetTempStream())
                        {
                            using (var destinationStream = AssetUtil.GetTempStream())
                            {
                                await assetStorage.DownloadAsync(assetId, asset.FileVersion, null, sourceStream);
                                sourceStream.Position = 0;

                                await assetThumbnailGenerator.CreateThumbnailAsync(sourceStream, destinationStream, width, height, mode);
                                destinationStream.Position = 0;

                                await assetStorage.UploadAsync(assetId, asset.FileVersion, assetSuffix, destinationStream);
                                destinationStream.Position = 0;

                                await destinationStream.CopyToAsync(bodyStream);
                            }
                        }
                    }
                }
                else
                {
                    await assetStorage.DownloadAsync(assetId, asset.FileVersion, null, bodyStream);
                }
            });
        }
    }
}

#pragma warning restore 1573