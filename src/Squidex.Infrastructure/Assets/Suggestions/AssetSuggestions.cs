// ==========================================================================
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
using Squidex.Config.CivicPlus;

namespace Squidex.Infrastructure.Assets.Suggestions
{
    public class AssetSuggestions : IAssetSuggestions
    {
        public AssetSuggestions(IAssetStore assetStore, IOptions<AuthenticationKeys> keys
            )
            : base(assetStore)
        {
            AzureResourceKey = keys.Value.AzureImageApi;
        }

        // Configuration:
        private string AzureResourceKey { get; }
        private string AzureEndpoint { get; } = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description,Adult";
        private double MinimumTagConfidence { get; } = 0.9;
        private double MinimumCaptionConfidence { get; } = 0.3;
        private double MaxImageSize { get; } = Math.Pow(1024, 2) * 4; // 4mb

        public async override Task<AssetFile> SuggestTagsAndDescription(AssetFile file)
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

            var suggestedTags =
                JObject.Parse(result)["tags"]
                       .ToObject<List<TagResult>>()
                       .Where(tag => tag.Confidence > MinimumTagConfidence)
                       .Select(tag => tag.Name)
                       .ToArray();

            var suggestedDescription =
                JObject.Parse(result)["description"]["captions"]
                       .ToObject<List<CaptionResult>>()
                       .OrderByDescending(tag => tag.Confidence)
                       .FirstOrDefault(tag => tag.Confidence > MinimumCaptionConfidence)
                       ?.Text ?? string.Empty;

            return new AssetFile(
                $"{file.FileName}.{file.FileExtension}",
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
                file.FileSize > MaxImageSize,
            }.Any(condition => !condition);

        private async Task<string> CallAzureService(AssetFile file)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AzureResourceKey);

            var response = await client.PostAsync(AzureEndpoint, await EncodeFile(file));

            if (response.Headers.Contains("Retry-After"))
            {
                // ref: https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-request-limits#waiting-before-sending-next-request
                var value = response.Headers.GetValues("Retry-After").First();
                System.Threading.Thread.Sleep(Convert.ToInt32(value));
                return await CallAzureService(file);
            }

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

        private sealed class TagResult
        {
            public string Name { get; set; }
            public double Confidence { get; set; }
        }

        private sealed class CaptionResult
        {
            public string Text { get; set; }
            public double Confidence { get; set; }
        }
    }
}
