// ==========================================================================
//  CsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Squidex.Infrastructure.CSV
{
    public class CsvConverter
    {
        public string ReadCsvToString(IFormFile file)
        {
            var csv = new List<string>();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    csv.Add(reader.ReadLine());
                }
            }

            var json = !csv.Any()
                ? null
                : Newtonsoft.Json.JsonConvert.SerializeObject(csv);

            return json;
        }
    }
}
