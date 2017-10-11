// ==========================================================================
//  WatsonTextSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class WatsonTextSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }
        public string Endpoint { get; } = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description,Adult";
        public double MinimumTagConfidence { get; } = 1;
        public double MinimumCaptionConfidence { get; } = 0;
        public double MaxFileSize { get; } = Math.Pow(1024, 3); // 1gb
    }
}
