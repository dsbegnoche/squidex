// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Squidex.Infrastructure.Log;
using Squidex.Infrastructure.Tasks;
using Xunit;

namespace Squidex.Infrastructure.UsageTracking
{
    public class ContentUsageTrackerTests
    {
        private readonly IContentUsageStore usageStore = A.Fake<IContentUsageStore>();
        private readonly ISemanticLog log = A.Fake<ISemanticLog>();
        private readonly ContentUsageTracker sut;
        private readonly Guid appId = Guid.NewGuid();

        public ContentUsageTrackerTests()
        {
            sut = new ContentUsageTracker(usageStore, log);
        }

        [Fact]
        public Task Should_throw_exception_if_contentIds_is_null()
        {
            return Assert.ThrowsAsync<ArgumentNullException>(() => sut.TrackAsync(null, DateTime.UtcNow, appId));
        }

        [Fact]
        public Task Should_throw_exception_if_contentIds_is_empty()
        {
            return Assert.ThrowsAsync<ArgumentException>(() => sut.TrackAsync(new List<Guid>(), DateTime.UtcNow, appId));
        }

        [Fact]
        public Task Should_throw_exception_if_appId_is_empty()
        {
            return Assert.ThrowsAsync<ArgumentException>(() => sut.TrackAsync(new List<Guid>(), DateTime.UtcNow, Guid.Empty));
        }

        [Fact]
        public async Task Should_store()
        {
            var now = DateTime.UtcNow;
            Guid contentId = Guid.NewGuid();
            List<Guid> contentIds = new List<Guid>()
            {
                contentId
            };

            await sut.TrackAsync(contentIds, now, appId);

            A.CallTo(() => usageStore.TrackUsagesAsync(contentIds, now, appId)).MustHaveHappened();
        }
    }
}
