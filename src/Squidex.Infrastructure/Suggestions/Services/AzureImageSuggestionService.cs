// ==========================================================================
//  AzureImageSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Squidex.Infrastructure.Assets;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class AzureImageSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public string Endpoint { get; set; } =
            "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description,Adult";

        public double MinimumTagConfidence { get; } = 0.9;

        public double MinimumCaptionConfidence { get; } = 0.3;

        public double MaxFileSize { get; } = Math.Pow(1024, 2) * 4; // 4mb

        public void InitializeService() => throw new NotImplementedException();

        public async Task<object> Analyze(object content)
        {
            var file = (AssetFile)content;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ResourceKey);

            var response = await client.PostAsync(Endpoint, await EncodeFile(file));

            if (response.Headers.Contains("Retry-After"))
            {
                // ref: https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-request-limits#waiting-before-sending-next-request
                var value = response.Headers.GetValues("Retry-After").First();
                System.Threading.Thread.Sleep(Convert.ToInt32(value));
                return await Analyze(file);
            }

            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<MultipartFormDataContent> EncodeFile(AssetFile file)
        {
            var content = new MultipartFormDataContent();

            using (var ms = new MemoryStream())
            {
                await file.OpenRead().CopyToAsync(ms);
                var byteContent = new ByteArrayContent(ms.ToArray());
                content.Add(byteContent, "File", "filename");
            }

            return content;
        }

        public string[] GetTags(object result)
        {
            return JObject.Parse((string)result)["tags"]
                .ToObject<List<ImageAssetSuggestions.TagResult>>()
                .Where(tag => tag.Confidence > MinimumTagConfidence)
                .Select(tag => tag.Name)
                .ToArray();
        }

        public string GetDescription(object result)
        {
            return JObject.Parse((string)result)["description"]["captions"]
                .ToObject<List<ImageAssetSuggestions.CaptionResult>>()
                .OrderByDescending(tag => tag.Confidence)
                .FirstOrDefault(tag => tag.Confidence > MinimumCaptionConfidence)
                ?.Text ?? string.Empty;
        }

        public bool IsAdultContent(object result)
        {
            return JObject.Parse((string)result)["adult"]["isAdultContent"].ToObject<bool>();
        }
    }
}
