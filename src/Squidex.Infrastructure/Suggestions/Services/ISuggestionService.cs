// ==========================================================================
//  ISuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Threading.Tasks;

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

        void InitializeService();

        Task<object> Analyze(object content);

        string[] GetTags(object result);

        string GetDescription(object result);

        bool IsAdultContent(object result);
    }
}
