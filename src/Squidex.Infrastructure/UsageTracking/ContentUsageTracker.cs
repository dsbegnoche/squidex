// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Timers;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class ContentUsageTracker : IContentUsageTracker
    {
        private readonly IContentUsageStore usageStore;
        private readonly ISemanticLog log;

        public ContentUsageTracker(IContentUsageStore usageStore, ISemanticLog log)
        {
            Guard.NotNull(usageStore, nameof(usageStore));
            Guard.NotNull(log, nameof(log));

            this.usageStore = usageStore;

            this.log = log;
        }

        public async Task TrackAsync(List<Guid> contentIds, DateTime accessDate)
        {
            Guard.NotEmpty(contentIds, nameof(contentIds));

            try
            {
                await usageStore.TrackUsagesAsync(contentIds, accessDate);
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "TrackUsage")
                    .WriteProperty("status", "Failed"));
            }
        }
    }
}
