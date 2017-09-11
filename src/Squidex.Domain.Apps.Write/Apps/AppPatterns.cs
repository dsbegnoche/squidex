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

        public IReadOnlyList<string> Patterns => patterns;

        public void Add(string name)
        {
            ThrowIfFound(name, () => "Cannot add pattern");

            patterns.Add(name);
        }

        private void ThrowIfFound(string name, Func<string> message)
        {
            if (patterns.Contains(name))
            {
                var error = new ValidationError("Pattern name is already assigned.", "Name");

                throw new ValidationException(message(), error);
            }
        }
    }
}
