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
    public class AzureImageSuggestionService : IImageSuggestionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public void InitializeService()
        {
        }

        public string Endpoint { get; set; } =
            "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description,Adult";

        public double MinimumTagConfidence { get; } = 0.9;

        public double MinimumCaptionConfidence { get; } = 0.3;

        public double MaxFileSize { get; } = Math.Pow(1024, 2) * 4; // 4mb

        public async Task<ServiceResults> Analyze(Stream fileStream)
        {
            if (fileStream.Length > MaxFileSize)
            {
                return new ServiceResults(null, null, null);
            }

            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ResourceKey);

            var response = await client.PostAsync(Endpoint, await EncodeFile(fileStream));

            if (response.Headers.Contains("Retry-After"))
            {
                // ref: https://docs.microsoft.com/en-us/azure/azure-resource-manager/resource-manager-request-limits#waiting-before-sending-next-request
                var value = response.Headers.GetValues("Retry-After").First();
                System.Threading.Thread.Sleep(Convert.ToInt32(value));
                return await Analyze(fileStream);
            }

            return ParseResult(await response.Content.ReadAsStringAsync());
        }

        private ServiceResults ParseResult(string result)
        {
            var tags = JObject.Parse(result)["tags"].ToObject<List<TagResult>>();
            var descriptions = JObject.Parse(result)["description"]["captions"].ToObject<List<DescriptionResult>>();
            var isAdultContent = JObject.Parse(result)["adult"]["isAdultContent"].ToObject<bool>();

            return new ServiceResults(FilterTags(tags), FilterCaptions(descriptions), isAdultContent);
        }

        private List<string> FilterTags(List<TagResult> tags) =>
                tags.Where(item => item.Confidence > MinimumTagConfidence)
                    .OrderByDescending(item => item.Confidence)
                    .Select(item => item.Name)
                    .ToList();

        private string FilterCaptions(List<DescriptionResult> tags) =>
                tags.Where(item => item.Confidence > MinimumCaptionConfidence)
                    .OrderByDescending(item => item.Confidence)
                    .Select(item => item.Text)
                    .FirstOrDefault();

        private static async Task<MultipartFormDataContent> EncodeFile(Stream fileStream)
        {
            var content = new MultipartFormDataContent();

            using (var ms = new MemoryStream())
            {
                await fileStream.CopyToAsync(ms);
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

        private sealed class DescriptionResult
        {
            public string Text { get; set; }
            public double Confidence { get; set; }
        }
    }
}
