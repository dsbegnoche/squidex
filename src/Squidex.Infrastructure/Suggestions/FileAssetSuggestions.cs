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
    public class FileAssetSuggestions : ITextSuggestions
    {
        public FileAssetSuggestions(IOptions<AuthenticationKeys> keys, ISuggestionService suggestionService)
        {
            switch (suggestionService)
            {
                case AzureTextSuggestionService _:
                    suggestionService.ResourceKey = keys.Value.AzureTextAnalyticsApi;
                    break;
                case WatsonTextSuggestionService _:
                    suggestionService.ResourceKey = keys.Value.WatsonLanguageApiPassword;
                    suggestionService.Username = keys.Value.WatsonLanguageApiUsername;
                    break;
            }

            SuggestionService = suggestionService;
            SuggestionService.InitializeService();
        }

        public ISuggestionService SuggestionService { get; set; }

        public async Task<AssetFile> SuggestTagsAndDescription(AssetFile file, string extension)
        {
            if (!ValidateFile(file))
            {
                return file;
            }

            var content = await GetTextFromFile(file, extension);
            var result = await SuggestionService.Analyze(content);

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
                file.FileSize < 0,
            }.Any(condition => !condition);

        private static async Task<string> GetTextFromFile(AssetFile file, string extension)
        {
            var content = string.Empty;
            switch (extension)
            {
                case "txt":
                    using (var sr = new StreamReader(file.OpenRead()))
                    {
                        content = await sr.ReadToEndAsync();
                        sr.Dispose();
                    }

                    break;
            }

            return content;
        }
    }
}
