// ==========================================================================
//  ICsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Write.FileConverter
{
    public interface IFileConverter
    {
        List<string[]> ReadFile(IFormFile file);

        string ReadWithSchema(ISchemaEntity schema, IFormFile file, string masterLanguage);
    }
}
