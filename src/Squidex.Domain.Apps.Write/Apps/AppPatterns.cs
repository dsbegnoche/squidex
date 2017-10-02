/*
 * CivicPlus implementation of Squidex Headless CMS
 */

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Domain.Apps.Core;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppPatterns
    {
        private readonly Dictionary<string, AppPattern> patterns = new Dictionary<string, AppPattern>();

        public IReadOnlyDictionary<string, AppPattern> Patterns => patterns;

        public void Add(string name, string pattern, string defaultMessage)
        {
            ThrowIfFound(name.ToLower(), pattern, () => "Cannot add pattern");
            var newPattern = new AppPattern
            {
                Name = name,
                Pattern = pattern,
                DefaultMessage = defaultMessage
            };

            patterns.Add(name.ToLower(), newPattern);
        }

        public void Remove(string name)
        {
            ThrowIfNotFound(name.ToLower());

            patterns.Remove(name.ToLower());
        }

        public void Update(string original, string name, string pattern, string defaultMessage)
        {
            Remove(original);
            Add(name, pattern, defaultMessage);
        }

        private void ThrowIfFound(string name, string pattern, Func<string> message)
        {
            if (patterns.ContainsKey(name))
            {
                var error = new ValidationError("Pattern name is already assigned.", "Name");

                throw new ValidationException(message(), error);
            }

            if (patterns.Values.Any(x => x.Pattern == pattern))
            {
                var error = new ValidationError("Pattern already exists.", "Pattern");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfNotFound(string name)
        {
            if (!patterns.ContainsKey(name))
            {
                throw new DomainObjectNotFoundException(name, "Patterns", typeof(AppDomainObject));
            }
        }
    }
}
