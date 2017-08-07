// ==========================================================================
//  ISchemaWebhookEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Read.Schemas
{
    public interface ISchemaWebhookEntity : ISchemaWebhookUrlEntity
    {
        Guid SchemaId { get; }

        long TotalSucceeded { get; }

        long TotalFailed { get; }

        long TotalTimedout { get; }

        long TotalRequestTime { get; }
    }
}
