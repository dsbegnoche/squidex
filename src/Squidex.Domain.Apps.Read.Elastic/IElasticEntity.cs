// ==========================================================================
//  IElasticEntity.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using NodaTime;

namespace Squidex.Domain.Apps.Read.Elastic
{
    public interface IElasticEntity
    {
        Guid Id { get; set; }

        Instant Created { get; set; }

        Instant LastModified { get; set; }
    }
}
