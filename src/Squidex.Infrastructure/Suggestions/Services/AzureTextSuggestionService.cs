// ==========================================================================
//  AzureTextSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class AzureTextSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public string Endpoint { get; set; } = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";

        public double MinimumTagConfidence { get; } = 1;

        public double MinimumCaptionConfidence { get; } = 0;

        public double MaxFileSize { get; } = Math.Pow(1024, 3) * 1; // 1gb

        public string TagKeyWord { get; } = "tags";

        public void InitializeService()
        {
            return;
        }

        public async Task<object> Analyze(string content)
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ResourceKey);

            var response = await client.PostAsync(Endpoint, EncodeFile(content));

            return await response.Content.ReadAsStringAsync();
        }

        public string[] GetTags(object result)
        {
            return JObject.Parse((string)result)["tags"]
                .ToObject<List<TagResult>>()
                .Where(tag => tag.Confidence > MinimumTagConfidence)
                .OrderBy(tag => tag.Confidence)
                .Take(4)
                .Select(tag => tag.Name)
                .ToArray();
        }

        public string GetDescription(object result)
        {
            return JObject.Parse((string)result)["description"]["captions"]
                       .ToObject<List<CaptionResult>>()
                       .OrderByDescending(caption => caption.Confidence)
                       .FirstOrDefault(caption => caption.Confidence > MinimumCaptionConfidence)
                       ?.Text ?? string.Empty;
        }

        private static MultipartFormDataContent EncodeFile(string content)
        {
            var data = new MultipartFormDataContent();
            var stringContent = new StringContent(content);
            data.Add(stringContent, "File", "filename");

            return data;
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
