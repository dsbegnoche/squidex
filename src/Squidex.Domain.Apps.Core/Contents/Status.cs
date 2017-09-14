// ==========================================================================
//  Status.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Contents
{
    public enum Status
    {
        Deleted = 0,
        Draft = 1,
        Submitted = 2,
        Declined = 3,
        Archived = 4,
        Published = 10
    }
}
