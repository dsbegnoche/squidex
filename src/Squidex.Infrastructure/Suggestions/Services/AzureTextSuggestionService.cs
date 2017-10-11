// ==========================================================================
//  AzureTextSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class AzureTextSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }
        public string Endpoint { get; } = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/keyPhrases";
        public double MinimumTagConfidence { get; } = 1;
        public double MinimumCaptionConfidence { get; } = 0;
        public double MaxFileSize { get; } = Math.Pow(1024, 3) * 1; // 1gb
    }
}
