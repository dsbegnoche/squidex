// ==========================================================================
//  AzureTextSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using Newtonsoft.Json;
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

        private TextAnalyticsAPI Client { get; set; }

        public void InitializeService()
        {
            Client = new TextAnalyticsAPI
            {
                AzureRegion = AzureRegions.Westus,
                SubscriptionKey = ResourceKey
            };
        }

        public async Task<object> Analyze(object content)
        {
            return await Client.KeyPhrasesAsync(EncodeFile((string)content));
        }

        public string[] GetTags(object result)
        {
            return ((KeyPhraseBatchResult)result).Documents
                .FirstOrDefault()
                .KeyPhrases.ToList()
                .Take(4)
                .ToArray();
        }

        public string GetDescription(object result) => string.Empty;

        private static MultiLanguageBatchInput EncodeFile(string content)
        {
            var data = new MultiLanguageBatchInput
            {
                Documents = new List<MultiLanguageInput>
                {
                    new MultiLanguageInput("en", "1", content)
                }
            };

            return data;
        }
    }
}
