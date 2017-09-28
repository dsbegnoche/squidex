using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.Assets.Suggestions
{
    public class AssetSuggestions : IAssetSuggestions
    {
        public AssetSuggestions(IAssetStore assetStore)
            : base(assetStore)
        {
        }

        public async override Task<AssetFile> SuggestTags(AssetFile file)
        {
            var azureResourceKey = "2b7aeed3711945b687a5342e0508a113";
            var azureEndpoint = "https://westus.api.cognitive.microsoft.com/vision/v1.0/tag";
            double minimumConfidence = 0.8;

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", azureResourceKey);

            var response = await client.PostAsync(azureEndpoint, await EncodeFile(file));
            var stringcontent = await response.Content.ReadAsStringAsync();

            var tags = JObject.Parse(stringcontent)["tags"].ToObject<List<TagResult>>();

            var suggestedTags = tags.Where(tag => tag.confidence > minimumConfidence)
                       .Select(tag => tag.name).ToArray();

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

        private sealed class TagResult
        {
            public string name { get; set; }
            public double confidence { get; set; }
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

        public override Task<string> SuggestSummary(AssetFile file)
        {
            throw new NotImplementedException();
        }
    }
}
