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

        public string Username { get; set; }

        public string Endpoint { get; set; } = "https://gateway.watsonplatform.net/natural-language-understanding/api";

        public double MinimumTagConfidence { get; } = 0.5;

        public double MinimumCaptionConfidence { get; } = 0.3;

        public double MaxFileSize { get; } = Math.Pow(1024, 3); // 1gb

        public string TagKeyWord { get; } = "keywords";
    }
}
