// ==========================================================================
//  AssetCommandMiddleware.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Write.Assets.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Infrastructure.Dispatching;
using System.Collections.Generic;

namespace Squidex.Domain.Apps.Write.Assets
{
    public class AssetCommandMiddleware : ICommandMiddleware
    {
        private readonly IAggregateHandler handler;
        private readonly IAssetStore assetStore;
        private readonly IAssetThumbnailGenerator assetThumbnailGenerator;

        public AssetCommandMiddleware(
            IAggregateHandler handler,
            IAssetStore assetStore,
            IAssetThumbnailGenerator assetThumbnailGenerator)
        {
            Guard.NotNull(handler, nameof(handler));
            Guard.NotNull(assetStore, nameof(assetStore));
            Guard.NotNull(assetThumbnailGenerator, nameof(assetThumbnailGenerator));

            this.handler = handler;
            this.assetStore = assetStore;
            this.assetThumbnailGenerator = assetThumbnailGenerator;
        }


        private void ValidateCond(bool condition, string message)
        {
            if (condition)
                throw new ValidationException("Cannot create asset.", new ValidationError(message));
        }

        private void CheckAssetFileAsync(AssetFile file)
        {
            var assetsConfig = file.AssetConfig;
            var filename = file.FileName;

            ValidateCond(file.FileSize > assetsConfig.MaxSize, $"File size cannot be longer than ${assetsConfig.MaxSize}.");

            ValidateCond(file.MaxAssetRepoSize > 0 && 
                         file.MaxAssetRepoSize < file.CurrentAssetRepoSize + file.FileSize, 
                         "You have reached your max asset size.");

            ValidateCond(!filename.Contains("."), "Asset has no extensions found");

            var extension = filename.Split('.').Last().ToLower();
            var validExtensions = AssetFileValidationConfig.ValidExtensions;

            ValidateCond(!validExtensions.Contains(extension), $"Asset extension '{extension}' is not an allowed filetype.");
        }

        protected async Task On(CreateAsset command, CommandContext context)
        {
            CheckAssetFileAsync(command.File);

            command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());
            try
            {
                var asset = await handler.CreateAsync<AssetDomainObject>(context, async a =>
                {
                    a.Create(command);

                    await assetStore.UploadTemporaryAsync(context.ContextId.ToString(), command.File.OpenRead());

                    context.Complete(EntityCreatedResult.Create(a.Id, a.Version));
                });

                await assetStore.CopyTemporaryAsync(context.ContextId.ToString(), asset.Id.ToString(), asset.FileVersion, null);
            }
            finally
            {
                await assetStore.DeleteTemporaryAsync(context.ContextId.ToString());
            }
        }

        protected async Task On(UpdateAsset command, CommandContext context)
        {
            CheckAssetFileAsync(command.File);
            command.ImageInfo = await assetThumbnailGenerator.GetImageInfoAsync(command.File.OpenRead());

            try
            {
                var asset = await handler.UpdateAsync<AssetDomainObject>(context, async a =>
                {
                    a.Update(command);

                    await assetStore.UploadTemporaryAsync(context.ContextId.ToString(), command.File.OpenRead());

                    context.Complete(new AssetSavedResult(a.Version, a.FileVersion));
                });

                await assetStore.CopyTemporaryAsync(context.ContextId.ToString(), asset.Id.ToString(), asset.FileVersion, null);
            }
            finally
            {
                await assetStore.DeleteTemporaryAsync(context.ContextId.ToString());
            }
        }

        protected Task On(RenameAsset command, CommandContext context)
        {
            return handler.UpdateAsync<AssetDomainObject>(context, a => a.Rename(command));
        }

        protected Task On(DeleteAsset command, CommandContext context)
        {
            return handler.UpdateAsync<AssetDomainObject>(context, a => a.Delete(command));
        }

        public async Task HandleAsync(CommandContext context, Func<Task> next)
        {
            if (!await this.DispatchActionAsync(context.Command, context))
            {
                await next();
            }
        }
    }
}
