using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public abstract class IAssetSuggestions
    {
#pragma warning disable SA1401 // Fields must be private
        internal IAssetStore AssetStore;
#pragma warning restore SA1401 // Fields must be private

        public IAssetSuggestions(IAssetStore assetStore)
        {
            AssetStore = assetStore;
        }

        public abstract Task<AssetFile> SuggestTags(AssetFile file);
        public abstract Task<string> SuggestSummary(AssetFile file);
    }
}
