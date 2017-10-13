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
using Squidex.Domain.Apps.Core.Schemas;
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

            // Read in file and use CsvTools to parse
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                var dt = DataTable.New.Read(reader);

                // Get headers and rows; convert any empty cell to null
                csv.Add(dt.ColumnNames
                    .ToList()
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToArray());

                csv.AddRange(dt.Rows.Select(row =>
                    {
                        return row.Values
                            .ToList()
                            .Any(v => !string.IsNullOrWhiteSpace(v))
                            ? row.Values.ToArray()
                            : null;
                    }).Where(r => r != null)
                    .ToList());

                // Return null if there is text in an extra column.
                if (csv.Any(c => c.Length > csv[0].Length))
                {
                    return null;
                }
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
            csv.Remove(headerRow);

            // Check the header row to ensure the field names align
            if (schemaFields.Any(f => !headerRow.Contains(f.Key)))
            {
                return null;
            }

            // Get the json of each row
            var elementsDictionary = new List<Dictionary<string, Dictionary<string, object>>>();
            csv.ForEach(row =>
            {
                var rowDictionary = new Dictionary<string, Dictionary<string, object>>();
                for (var col = 0; col < headerRow.Length; col++)
                {
                    var languageDictionary = new Dictionary<string, object>();
                    var field = schemaFields.First(f => f.Key == headerRow[col]).Value;
                    var languageCode = field.Paritioning.Key == "invariant"
                                        ? "iv"
                                        : masterLanguage;

                    row[col] = row[col].Trim('"');
                    switch (field)
                    {
                        case TagField _:
                            var tags = !string.IsNullOrWhiteSpace(row[col].Trim())
                                ? row[col].Trim().Split(',').ToList().Distinct().ToArray()
                                : null;
                            languageDictionary.Add(languageCode, tags);
                            break;
                        case IReferenceField _ when Guid.TryParse(row[col], out var reference):
                            var referenceGuid = new Guid[] { reference };
                            languageDictionary.Add(languageCode, referenceGuid);
                            break;
                        default:
                            languageDictionary.Add(languageCode, !string.IsNullOrWhiteSpace(row[col].Trim()) ? row[col].Trim() : null);
                            break;
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
