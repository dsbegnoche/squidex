// ==========================================================================
//  CsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Write.FileConverter
{
    public class CsvConverter : IFileConverter
    {
        public async Task<List<string[]>> ReadAsync(IFormFile file)
        {
            var csv = new List<string>();
            if (file.Length <= 0)
            {
                return null;
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                {
                    csv.Add(await reader.ReadLineAsync());
                }
            }

            var csvSplit = csv.Select(r => r.Split(',')).ToList();
            return csvSplit;
        }

        public async Task<string> ReadWithSchemaAsync(ISchemaEntity schema, IFormFile file, string masterLanguage)
        {
            var csv = await ReadAsync(file);

            if (schema == null || csv == null || !csv.Any())
            {
                return null;
            }

            var schemaFields = schema.SchemaDef.FieldsByName.ToList();
            var headerRow = csv[0];
            var tagsColumn = Array.IndexOf(headerRow, "tags");
            csv.Remove(headerRow);

            if (schemaFields.Any(f => !headerRow.Contains(f.Key)))
            {
                return null;
            }

            var elementsDictionary = new List<Dictionary<string, Dictionary<string, object>>>();

            csv.ForEach(row =>
            {
                var rowDictionary = new Dictionary<string, Dictionary<string, object>>();
                for (var col = 0; col < headerRow.Length; col++)
                {
                    var languageDicionary = new Dictionary<string, object>();

                    if (col == tagsColumn)
                    {
                        languageDicionary.Add(masterLanguage, row[col].Split('|'));
                    }
                    else
                    {
                        languageDicionary.Add(masterLanguage, row[col]);
                    }
                    rowDictionary.Add(headerRow[col], languageDicionary);
                }
                elementsDictionary.Add(rowDictionary);
            });

            var json = !elementsDictionary.Any()
                ? null
                : Newtonsoft.Json.JsonConvert.SerializeObject(elementsDictionary);

            return json;
        }
    }
}
