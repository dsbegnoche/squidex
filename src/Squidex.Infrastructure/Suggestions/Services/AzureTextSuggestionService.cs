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
    public class AzureTextSuggestionService : ITextSuggesionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public void InitializeService()
        {
            Client = new TextAnalyticsAPI
            {
                AzureRegion = AzureRegions.Westus,
                SubscriptionKey = ResourceKey
            };
        }

        public string Endpoint { get; set; } =
            "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";

        private TextAnalyticsAPI Client { get; set; }

        public int TagsToKeep { get; } = 5;

        public async Task<ServiceResults> Analyze(string content)
        {
            var result = await Client.KeyPhrasesAsync(EncodeFile(content));

            var tags =
                result.Documents.FirstOrDefault()
                      .KeyPhrases
                      .Take(TagsToKeep)
                      .ToList();

            return new ServiceResults(tags, null, null);
        }

        private static MultiLanguageBatchInput EncodeFile(string content) =>
            new MultiLanguageBatchInput
            {
                Documents = new List<MultiLanguageInput>
                {
                    new MultiLanguageInput("en", "1", content)
                }
            };
    }
}
