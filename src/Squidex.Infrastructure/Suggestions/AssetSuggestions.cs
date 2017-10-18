// ==========================================================================
//  FileAssetSuggestions.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Suggestions.Services;

namespace Squidex.Infrastructure.Suggestions
{
    public class AssetSuggestions : IAssetSuggestions
    {
        public AssetSuggestions(IOptions<AuthenticationKeys> keys,
            IImageSuggestionService imageSuggestionService,
            ITextSuggesionService textSuggestionService)
        {
            switch (textSuggestionService)
            {
                case AzureTextSuggestionService _:
                    textSuggestionService.ResourceKey = keys.Value.AzureTextAnalyticsApi;
                    break;

                case WatsonTextSuggestionService _:
                    textSuggestionService.ResourceKey = keys.Value.WatsonLanguageApiPassword;
                    textSuggestionService.Username = keys.Value.WatsonLanguageApiUsername;
                    break;
            }

            switch (imageSuggestionService)
            {
                case AzureImageSuggestionService _:
                    imageSuggestionService.ResourceKey = keys.Value.AzureImageApi;
                    break;
            }

            textSuggestionService.InitializeService();
            imageSuggestionService.InitializeService();

            TextSuggestionService = textSuggestionService;
            ImageSuggestionService = imageSuggestionService;
        }

        private ITextSuggesionService TextSuggestionService { get; set; }
        private IImageSuggestionService ImageSuggestionService { get; set; }

        public async Task<AssetFile> SuggestTagsAndDescription(AssetFile file, bool isImage, Stream compressedStream)
        {
            ServiceResults results = null;

            try
            {
                results = await Process(file, isImage, compressedStream);
            }
            catch
            {
                results = null;
            }

            if (results == null)
            {
                return file;
            }

            if (results.IsAdultContent ?? false)
            {
                var error = new ValidationError("Adult content found in asset upload");
                throw new ValidationException("Cannot create asset.", error);
            }

            return new AssetFile(
                    file.FileName,
                    file.MimeType,
                    file.FileSize,
                    file.OpenRead,
                    results.Description ?? string.Empty,
                    results.Tags?.ToArray() ?? new string[0],
                    file.AssetConfig,
                    file.MaxAssetRepoSize,
                    file.CurrentAssetRepoSize
                );
        }

        private async Task<ServiceResults> Process(AssetFile file, bool isImage, Stream compressedStream)
        {
            if (isImage)
            {
                return
                    await ImageSuggestionService.Analyze(
                        file.OpenRead().Length > ImageSuggestionService.MaxFileSize ?
                            compressedStream :
                            file.OpenRead());
            }
            else if (file.FileExtension == "txt")
            {
                using (var sr = new StreamReader(file.OpenRead()))
                {
                    return await TextSuggestionService.Analyze(await sr.ReadToEndAsync());
                }
            }

            return null;
        }
    }
}
