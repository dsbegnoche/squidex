using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Read.Contents
{
    public static class Extensions
    {
        private const int MaxLength = 1024 * 1024;

        public static List<Guid> ToReferencedIds(this IdContentData data, Schema schema)
        {
            return data.GetReferencedIds(schema).ToList();
        }

        public static string ToFullText<T>(this ContentData<T> data)
        {
            var stringBuilder = new StringBuilder();

            foreach (var text in data.Values.SelectMany(x => x.Values).Where(x => x != null).OfType<JValue>())
            {
                if (text.Type == JTokenType.String)
                {
                    var value = text.ToString();

                    if (value.Length < 1000)
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(text);
                    }
                }
            }

            var result = stringBuilder.ToString();

            if (result.Length > MaxLength)
            {
                result = result.Substring(MaxLength);
            }

            return result;
        }

        public static NamedContentData ToData(this IdContentData data, Schema schema, List<Guid> deletedIds)
        {
            return data
                .ToCleanedReferences(schema, new HashSet<Guid>(deletedIds ?? new List<Guid>()))
                .ToNameModel(schema, true);
        }
    }
}
