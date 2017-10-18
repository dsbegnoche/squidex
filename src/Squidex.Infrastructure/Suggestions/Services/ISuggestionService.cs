// ==========================================================================
//  ISuggestionService.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Suggestions.Services
{
    public interface ISuggestionService
    {
        string ResourceKey { get; set; }

        string Username { get; set; }

        // needed so can have values of resource and username
        void InitializeService();

        string Endpoint { get; set; }
    }

    public interface ITextSuggesionService : ISuggestionService
    {
        Task<ServiceResults> Analyze(string content);
    }

    public interface IImageSuggestionService : ISuggestionService
    {
        double MaxFileSize { get; }
        Task<ServiceResults> Analyze(Stream content);
    }
}
