// ==========================================================================
//  ImageAssetSuggestions.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Suggestions.Services;

namespace Squidex.Infrastructure.Suggestions
{
    public class ImageAssetSuggestions : IAssetSuggestions
    {
        public ImageAssetSuggestions(IOptions<AuthenticationKeys> keys, ISuggestionService suggestionService)
        {
            if (suggestionService is AzureImageSuggestionService)
            {
                suggestionService.ResourceKey = keys.Value.AzureImageApi;
            }

            SuggestionService = suggestionService;
        }

        public ISuggestionService SuggestionService { get; set; }

        public async Task<AssetFile> SuggestTagsAndDescription(AssetFile file)
        {
            if (!ValidateFile(file))
            {
                return file;
            }

            var result = await SuggestionService.Analyze(file);

            if (SuggestionService.IsAdultContent(result))
            {
                var error = new ValidationError("Adult content found in asset upload");
                throw new ValidationException("Cannot create asset.", error);
            }

            var suggestedTags = SuggestionService.GetTags(result);
            var suggestedDescription = SuggestionService.GetDescription(result);

            return new AssetFile(
                file.FileName,
                file.MimeType,
                file.FileSize,
                file.OpenRead,
                suggestedDescription,
                suggestedTags,
                file.AssetConfig,
                file.MaxAssetRepoSize,
                file.CurrentAssetRepoSize
                );
        }

        private bool ValidateFile(AssetFile file) => new[]
            {
                file.FileSize > SuggestionService.MaxFileSize,
            }.Any(condition => !condition);

        internal sealed class TagResult
        {
            public string Name { get; set; }
            public double Confidence { get; set; }
        }

        internal sealed class CaptionResult
        {
            public string Text { get; set; }
            public double Confidence { get; set; }
        }
    }
}
