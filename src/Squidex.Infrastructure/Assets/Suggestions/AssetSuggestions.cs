using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.Assets.Suggestions
{
    public class AssetSuggestions : IAssetSuggestions
    {
        public AssetSuggestions(IAssetStore assetStore)
            : base(assetStore)
        {
        }

        public async override Task<List<string>> SuggestedTags(string assetId, int assetVersion)
        {
            var azureResourceKey = "2b7aeed3711945b687a5342e0508a113";
            var azureEndpoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/tag";
            double minimumConfidence = 0.8;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureResourceKey);

            // pending merge:
            // var assetStream = AssetUtil.GetTempStream();
            FileStream assetStream = null;

            await AssetStore.DownloadAsync(assetId, assetVersion, null, assetStream);

            var response = await client.PostAsync(azureEndpoint, await EncodeFile(assetStream));

            var tags =
                ((JObject)await response.Content.ReadAsStringAsync())
                    .SelectToken("tags")
                    .ToObject<List<TagResult>>();

            return tags.Where(tag => tag.Confidence > minimumConfidence)
                       .Select(tag => tag.Name).ToList();
        }

        private sealed class TagResult
        {
            public string Name { get; set; }
            public double Confidence { get; set; }
        }

        // TODO
        private async Task<MultipartFormDataContent> EncodeFile(Stream file)
        {
            var content = new MultipartFormDataContent();
            // var fileContentTask = new StreamContent(info.OpenRead()).ReadAsByteArrayAsync();
            // var fileContent = Task.Run(() => fileContentTask).Result;
            content.Add(new StreamContent(file), "File", "filename");
            return await Task.Run(() => content);
        }

        public override Task<string> SuggestedSummary(string assetId, int assetVersion)
        {
            throw new NotImplementedException();
        }
    }
}
