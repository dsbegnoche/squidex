/*
 * CivicPlus implementation of Squidex Headless CMS
 */

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Write.Apps
{
    public class AppPatterns
    {
        private readonly Dictionary<string, string> patterns = new Dictionary<string, string>();

        public IReadOnlyDictionary<string, string> Patterns => patterns;

        public void Add(string name, string pattern)
        {
            ThrowIfFound(name.ToLower(), () => "Cannot add pattern");

            patterns.Add(name.ToLower(), pattern);
        }

        public void Remove(string name)
        {
            ThrowIfNotFound(name.ToLower());

            patterns.Remove(name.ToLower());
        }

        public void Update(string original, string name, string pattern)
        {
            if (original.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            Remove(original);
            Add(name, pattern);
        }

        private void ThrowIfFound(string name, Func<string> message)
        {
            if (patterns.ContainsKey(name))
            {
                var error = new ValidationError("Pattern name is already assigned.", "Name");

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
