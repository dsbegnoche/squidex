// ==========================================================================
//  StatusFlow.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Shared.Identity;

namespace Squidex.Domain.Apps.Core.Contents
{
    public static class StatusFlow
    {
        private static readonly Dictionary<Status, Dictionary<Status, string[]>> Flow =
            new Dictionary<Status, Dictionary<Status, string[]>>
            {
                [Status.Draft] = new Dictionary<Status, string[]>
                {
                    [Status.Published] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    },
                    [Status.Submitted] = new[]
                    {
                        SquidexRoles.AppAuthor
                    },
                    [Status.Archived] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    }
                },
                [Status.Published] = new Dictionary<Status, string[]>
                {
                    [Status.Draft] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    },
                    [Status.Archived] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    }
                },
                [Status.Submitted] = new Dictionary<Status, string[]>
                {
                    [Status.Declined] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    },
                    [Status.Published] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    },
                    [Status.Archived] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    }
                },
                [Status.Declined] = new Dictionary<Status, string[]>
                {
                    [Status.Archived] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    },
                    [Status.Published] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    },
                    [Status.Submitted] = new[] { SquidexRoles.AppAuthor }
                },
                [Status.Archived] = new Dictionary<Status, string[]>
                {
                    [Status.Draft] = new[]
                    {
                        SquidexRoles.Administrator, SquidexRoles.AppOwner, SquidexRoles.AppDeveloper,
                        SquidexRoles.AppEditor
                    }
                }
            };

        public static bool Exists(Status status)
        {
            return Flow.ContainsKey(status);
        }

        public static bool CanChange(Status status, Status toStatus, List<string> roles)
        {
            return Flow.TryGetValue(status, out var state) && state.TryGetValue(toStatus, out var access)
                   && access.Any(roles.Contains);
        }
    }
}