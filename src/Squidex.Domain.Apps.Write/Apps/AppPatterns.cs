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
        private readonly List<string> patterns = new List<string>();

        public void Add(string name)
        {
            ThrowIfFound(name.ToLower(), () => "Cannot add pattern");

            patterns.Add(name.ToLower());
        }

        public void Remove(string name)
        {
            ThrowIfNotFound(name.ToLower());

            patterns.Remove(name.ToLower());
        }

        public void Update(string original, string name)
        {
            if (original.Equals(name, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            Remove(original);
            Add(name);
        }

        private void ThrowIfFound(string name, Func<string> message)
        {
            if (patterns.Contains(name))
            {
                var error = new ValidationError("Pattern name is already assigned.", "Name");

                throw new ValidationException(message(), error);
            }
        }

        private void ThrowIfNotFound(string name)
        {
            if (!patterns.Contains(name))
            {
                throw new DomainObjectNotFoundException(name, "Patterns", typeof(AppDomainObject));
            }
        }
    }
}
