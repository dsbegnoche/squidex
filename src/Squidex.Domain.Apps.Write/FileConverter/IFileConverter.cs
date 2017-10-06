// ==========================================================================
//  ICsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Write.FileConverter
{
    public interface IFileConverter
    {
        Task<string> ReadAsync(IFormFile file);

        Task<string> ReadWithSchemaAsync(ISchemaEntity schema, IFormFile file);
    }
}
