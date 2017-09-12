// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public interface IContentUsageTracker
    {
        Task TrackAsync(List<Guid> contentIds, DateTime accessDate);
    }
}
