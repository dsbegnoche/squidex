﻿// ==========================================================================
//  IAppEntity.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core;

namespace Squidex.Domain.Apps.Read.Apps
{
    public interface IAppEntity : IEntity, IEntityWithVersion
    {
        string Name { get; }

        bool IsDeleted { get; } 

        string PlanId { get; }

        string PlanOwner { get; }

        LanguagesConfig LanguagesConfig { get; }

        IReadOnlyCollection<IAppClientEntity> Clients { get; }

        IReadOnlyCollection<IAppContributorEntity> Contributors { get; }

        PartitionResolver PartitionResolver { get; }
    }
}
