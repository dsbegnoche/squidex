// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.UsageTracking
{
    public interface IContentUsageStore
    {
        Task TrackUsagesAsync(List<Guid> contentIds, DateTime accessDate, Guid appId);
    }
}
