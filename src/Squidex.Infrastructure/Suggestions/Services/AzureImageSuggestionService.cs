// ==========================================================================
//  AzureImageSuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public class AzureImageSuggestionService : ISuggestionService
    {
        public string ResourceKey { get; set; }

        public string Username { get; set; }

        public string Endpoint { get; set; } = "https://westus.api.cognitive.microsoft.com/vision/v1.0/analyze?visualFeatures=Tags,Description,Adult";

        public double MinimumTagConfidence { get; } = 0.9;

        public double MinimumCaptionConfidence { get; } = 0.3;

        public double MaxFileSize { get; } = Math.Pow(1024, 2) * 4; // 4mb

        public string TagKeyWord { get; } = "tags";

        public void InitializeService()
        {
            return;
        }

        public async Task<object> Analyze(string content) => throw new NotImplementedException();

        public string[] GetTags(object result) => throw new NotImplementedException();

        public string GetDescription(object result) => throw new NotImplementedException();
    }
}
