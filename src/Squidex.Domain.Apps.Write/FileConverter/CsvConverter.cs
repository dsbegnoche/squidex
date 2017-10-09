// ==========================================================================
//  CsvConverter.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataAccess;
using Microsoft.AspNetCore.Http;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Write.FileConverter
{
    public class CsvConverter : IFileConverter
    {
        public List<string[]> ReadFile(IFormFile file)
        {
            var csv = new List<string[]>();
            if (file.Length <= 0)
            {
                return null;
            }

            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var dt = DataTable.New.Read(reader);
                csv.Add(dt.ColumnNames.ToArray());
                csv.AddRange(dt.Rows.Select(row =>
                    {
                        return row.Values
                            .ToList()
                            .Any(v => !string.IsNullOrWhiteSpace(v))
                            ? row.Values.ToArray()
                            : null;
                    }).Where(r => r != null)
                    .ToList());
            }

            return csv;
        }

        public string ReadWithSchema(ISchemaEntity schema, IFormFile file, string masterLanguage)
        {
            // Check for empty file
            var csv = ReadFile(file);
            if (schema == null || csv == null || !csv.Any())
            {
                return null;
            }

            // Get the schema fields and header row. Check that the header row is correct.
            var schemaFields = schema.SchemaDef.FieldsByName.ToList();
            var headerRow = csv[0];
            var tagsColumn = Array.IndexOf(headerRow, "tags");
            csv.Remove(headerRow);

            if (schemaFields.Any(f => !headerRow.Contains(f.Key)))
            {
                return null;
            }

            // Get the json of each row (only in master lanuage)
            var elementsDictionary = new List<Dictionary<string, Dictionary<string, object>>>();
            csv.ForEach(row =>
            {
                var rowDictionary = new Dictionary<string, Dictionary<string, object>>();
                for (var col = 0; col < headerRow.Length; col++)
                {
                    var languageDictionary = new Dictionary<string, object>();
                    var languageCode =
                        schemaFields.First(f => f.Key == headerRow[col]).Value.Paritioning.Key == "invariant"
                            ? "iv"
                            : masterLanguage;

                    if (row[col].StartsWith("\"", StringComparison.InvariantCultureIgnoreCase)
                        && row[col].EndsWith("\"", StringComparison.InvariantCultureIgnoreCase))
                    {
                        row[col] = row[col].Trim('"');
                    }

                    if (col == tagsColumn)
                    {
                        languageDictionary.Add(languageCode, !string.IsNullOrWhiteSpace(row[col].Trim()) ? row[col].Trim().Split(',') : null);
                    }
                    else
                    {
                        languageDictionary.Add(languageCode, !string.IsNullOrWhiteSpace(row[col].Trim()) ? row[col].Trim() : null);
                    }
                    rowDictionary.Add(headerRow[col], languageDictionary);
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
