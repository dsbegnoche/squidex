// ==========================================================================
//  ISuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

namespace Squidex.Infrastructure.Suggestions.Services
{
    public interface ISuggestionService
    {
        string ResourceKey { get; set; }
        string Endpoint { get; }
        double MinimumTagConfidence { get; }
        double MinimumCaptionConfidence { get; }
        double MaxFileSize { get; }
    }
}
