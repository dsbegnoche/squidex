// ==========================================================================
//  CsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Infrastructure.FileConverter.Base;

namespace Squidex.Infrastructure.FileConverter
{
    public class CsvConverter : IFileConverter
    {
        public async Task<string> ReadAsync(IFormFile file)
        {
            var csv = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    csv.Add(await reader.ReadLineAsync());
                }
            }

            var json = !csv.Any()
                ? null
                : Newtonsoft.Json.JsonConvert.SerializeObject(csv);

            return json;
        }
    }
}
