// ==========================================================================
//  IContentEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Read.Contents
{
    public interface IContentEntity : IAppRefEntity, IEntityWithCreatedBy, IEntityWithLastModifiedBy, IEntityWithVersion
    {
        //TODO: Remove??;
        bool IsPublished { get; }

        Status Status { get; }

        NamedContentData Data { get; }
    }
}
