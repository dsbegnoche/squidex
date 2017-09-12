// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;

namespace Squidex.Infrastructure.UsageTracking
{
    public sealed class MongoContentUsageStore : MongoRepositoryBase<MongoContentUsage>, IContentUsageStore
    {
        private static readonly UpdateOptions Upsert = new UpdateOptions { IsUpsert = true };

        public MongoContentUsageStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Projections_ContentUsage";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoContentUsage> collection)
        {
            return collection.Indexes.CreateOneAsync(Index.Ascending(x => x.ContentId).Ascending(x => x.AccessDate));
        }

        public async Task TrackUsagesAsync(List<Guid> contentIds, DateTime accessDate)
        {
            var requests = new List<WriteModel<MongoContentUsage>>();

            foreach (Guid contentId in contentIds)
            {
                requests.Add(new UpdateOneModel<MongoContentUsage>(
                    Builders<MongoContentUsage>.Filter.Eq(x => x.ContentId, contentId),
                    Update.Set(x => x.AccessDate, accessDate).SetOnInsert(x => x.ContentId, contentId))
                {
                    IsUpsert = true
                });
            }

            await Collection.BulkWriteAsync(requests);
        }
    }
}
