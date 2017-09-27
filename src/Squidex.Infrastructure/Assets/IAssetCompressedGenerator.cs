// ==========================================================================
//  IAssetCompressedGenerator.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.IO;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Assets
{
    public interface IAssetCompressedGenerator
    {
        Task CreateCompressedAsync(Stream source, Stream destination);
    }
}