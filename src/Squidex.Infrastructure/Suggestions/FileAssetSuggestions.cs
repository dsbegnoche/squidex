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
        public FileAssetSuggestions(IOptions<AuthenticationKeys> keys, ISuggestionService suggestionService)
        {
            switch (suggestionService)
            {
                case AzureTextSuggestionService _:
                    suggestionService.ResourceKey = keys.Value.AzureTextAnalyticsApi;
                    break;
                case WatsonTextSuggestionService _:
                    suggestionService.ResourceKey = keys.Value.WatsonLanguageApi;
                    break;
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

            var result = await CallAzureService(file);

            var isAdultContent =
                JObject.Parse(result)["adult"]["isAdultContent"]
                       .ToObject<bool>();

            if (isAdultContent)
            {
                var error = new ValidationError("Adult content found in asset upload");
                throw new ValidationException("Cannot create asset.", error);
            }

            return new AssetFile(
                file.FileName,
                file.MimeType,
                file.FileSize,
                file.OpenRead,
                string.Empty,
                new string[0],
                file.AssetConfig,
                file.MaxAssetRepoSize,
                file.CurrentAssetRepoSize
                );
        }

        private bool ValidateFile(AssetFile file) => new[]
            {
                file.FileSize < 0,
            }.Any(condition => !condition);

        private async Task<string> CallAzureService(AssetFile file)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SuggestionService.ResourceKey);

            var response = await client.PostAsync(SuggestionService.Endpoint, await EncodeFile(file));

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<MultipartFormDataContent> EncodeFile(AssetFile file)
        {
            var content = new MultipartFormDataContent();

            using (MemoryStream ms = new MemoryStream())
            {
                await file.OpenRead().CopyToAsync(ms);
                var byteContent = new ByteArrayContent(ms.ToArray());
                content.Add(byteContent, "File", "filename");
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
