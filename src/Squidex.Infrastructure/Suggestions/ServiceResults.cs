using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Squidex.Infrastructure.Suggestions.Services;

namespace Squidex.Infrastructure.Suggestions
{
    /// <summary> result of analyzing an asset with a service. null value indicates unsupported </summary>
    public class ServiceResults
    {
        public List<string> Tags { get; }
        public string Description { get; }
        public bool? IsAdultContent { get; }

        public ServiceResults(List<string> tags,
            string description,
            bool? isAdultContent)
        {
            Tags = tags;
            Description = description;
            IsAdultContent = isAdultContent;
        }
    }
}
