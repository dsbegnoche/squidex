// ==========================================================================
//  ISuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

namespace Squidex.Infrastructure.Suggestions.Services
{
    public interface ISuggestionService
    {
        string ResourceKey { get; set; }

        string Username { get; set; }

        string Endpoint { get; set; }

        double MinimumTagConfidence { get; }

        double MinimumCaptionConfidence { get; }

        double MaxFileSize { get; }

        string TagKeyWord { get; }
    }
}
