// ==========================================================================
//  ISchemaProvider.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Read.Schemas.Services
{
    public interface ISchemaProvider
    {
        Task<IReadOnlyList<ISchemaEntity>> QueryAllAsync(Guid appId);

        Task<ISchemaEntity> FindSchemaByIdAsync(Guid id, bool provideDeleted = false);

        Task<ISchemaEntity> FindSchemaByNameAsync(Guid appId, string name);
    }
}
