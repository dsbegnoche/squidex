// ==========================================================================
//  ICsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Squidex.Infrastructure.FileConverter.Base
{
    public interface IFileConverter
    {
        Task<string> ReadAsync(IFormFile file);
    }
}
