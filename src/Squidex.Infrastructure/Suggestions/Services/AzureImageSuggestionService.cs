// ==========================================================================
//  AzureImageSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class AzureImageSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public string Endpoint { get; } = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description,Adult";

        public double MinimumTagConfidence { get; } = 0.9;

        public double MinimumCaptionConfidence { get; } = 0.3;

        public double MaxFileSize { get; } = Math.Pow(1024, 2) * 4; // 4mb

        public string TagKeyWord { get; } = "tags";
    }
}
