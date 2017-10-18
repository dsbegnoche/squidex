﻿// ==========================================================================
//  IAssetSuggestions.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Assets;
using Squidex.Infrastructure.Suggestions.Services;

namespace Squidex.Infrastructure.Suggestions
{
    public interface IAssetSuggestions
    {
        Task<AssetFile> SuggestTagsAndDescription(AssetFile file);

        ISuggestionService SuggestionService { get; set; }
    }
}