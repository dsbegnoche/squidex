// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        private string AzureEndpoint { get; } = "https://westus.api.cognitive.microsoft.com/vision/v1.0/tag";
        private double MinimumConfidence { get; } = 0.8;
        private double MaxImageSize { get; } = Math.Pow(1024, 2) * 4; // 4mb

        public async override Task<AssetFile> SuggestTags(AssetFile file)
        {
            if (!ValidateFile(file))
            {
                return file;
            }

            var suggestedTags =
                JObject.Parse(await CallAzureService(file))["tags"]
                       .ToObject<List<TagResult>>()
                       .Where(tag => tag.Confidence > MinimumConfidence)
                       .Select(tag => tag.Name)
                       .ToArray();

            return new AssetFile(
                file.FileName,
                file.MimeType,
                file.FileSize,
                file.OpenRead,
                file.BriefDescription,
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

        public override Task<string> SuggestSummary(AssetFile file) =>
            throw new NotImplementedException();
    }
}
