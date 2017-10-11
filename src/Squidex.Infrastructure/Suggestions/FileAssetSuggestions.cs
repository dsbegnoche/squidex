// ==========================================================================
//  FileAssetSuggestions.cs
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
    public class FileAssetSuggestions : ITextSuggestions
    {
        private readonly string serviceTagKey = string.Empty;

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
        }

        public ISuggestionService SuggestionService { get; set; }

        public async Task<AssetFile> SuggestTagsAndDescription(AssetFile file, string extension)
        {
            if (!ValidateFile(file))
            {
                return file;
            }

            var content = await GetTextFromFile(file, extension);
            var result = await CallTextAnalysisService(content);

            var suggestedTags =
                JObject.Parse(result)[SuggestionService.TagKeyWord]
                    .ToObject<List<TagResult>>()
                    .Where(tag => tag.Confidence > SuggestionService.MinimumTagConfidence)
                    .OrderBy(tag => tag.Confidence)
                    .Take(4)
                    .Select(tag => tag.Name)
                    .ToArray();

            var suggestedDescription =
                JObject.Parse(result)["description"]["captions"]
                    .ToObject<List<CaptionResult>>()
                    .OrderByDescending(caption => caption.Confidence)
                    .FirstOrDefault(caption => caption.Confidence > SuggestionService.MinimumCaptionConfidence)
                    ?.Text ?? string.Empty;

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

        private async Task<string> CallTextAnalysisService(string content)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SuggestionService.ResourceKey);

            var response = await client.PostAsync(SuggestionService.Endpoint, EncodeFile(content));

            return await response.Content.ReadAsStringAsync();
        }

        private static MultipartFormDataContent EncodeFile(string content)
        {
            var data = new MultipartFormDataContent();
            var stringContent = new StringContent(content);
            data.Add(stringContent, "File", "filename");

            return data;
        }

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
