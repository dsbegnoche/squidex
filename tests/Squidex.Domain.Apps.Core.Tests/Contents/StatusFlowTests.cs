// ==========================================================================
//  StatusFlowTests.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Shared.Identity;
using Xunit;

namespace Squidex.Domain.Apps.Core.Contents
{
    public class StatusFlowTests
    {
        [Fact]
        public void Should_make_tests()
        {
            Assert.True(StatusFlow.Exists(Status.Draft));
            Assert.True(StatusFlow.Exists(Status.Archived));
            Assert.True(StatusFlow.Exists(Status.Published));
            Assert.True(StatusFlow.Exists(Status.Submitted));
            Assert.True(StatusFlow.Exists(Status.Declined));

            Assert.True(StatusFlow.CanChange(Status.Draft, Status.Archived, new List<string> { SquidexRoles.Administrator, SquidexRoles.AppOwner }));
            Assert.True(StatusFlow.CanChange(Status.Draft, Status.Published, new List<string> { SquidexRoles.Administrator, SquidexRoles.AppOwner }));

            Assert.True(StatusFlow.CanChange(Status.Published, Status.Draft, new List<string> { SquidexRoles.Administrator, SquidexRoles.AppOwner }));
            Assert.True(StatusFlow.CanChange(Status.Published, Status.Archived, new List<string> { SquidexRoles.Administrator, SquidexRoles.AppOwner }));

            Assert.True(StatusFlow.CanChange(Status.Archived, Status.Draft, new List<string> { SquidexRoles.Administrator, SquidexRoles.AppOwner }));

            Assert.False(StatusFlow.Exists((Status)int.MaxValue));
            Assert.False(StatusFlow.CanChange(Status.Archived, Status.Published, new List<string> { SquidexRoles.Administrator, SquidexRoles.AppOwner }));
        }

        [Fact]
        public void Can_Change_Submitted()
        {
            Assert.True(StatusFlow.CanChange(Status.Submitted, Status.Declined, new List<string> { SquidexRoles.AppEditor }));
            Assert.True(StatusFlow.CanChange(Status.Submitted, Status.Published, new List<string> { SquidexRoles.AppEditor }));
            Assert.False(StatusFlow.CanChange(Status.Submitted, Status.Declined, new List<string> { SquidexRoles.AppAuthor }));
        }

        [Fact]
        public void Can_Change_Declined()
        {
            Assert.True(StatusFlow.CanChange(Status.Declined, Status.Published, new List<string> { SquidexRoles.AppEditor }));
            Assert.True(StatusFlow.CanChange(Status.Declined, Status.Submitted, new List<string> { SquidexRoles.AppAuthor }));
            Assert.False(StatusFlow.CanChange(Status.Declined, Status.Published, new List<string> { SquidexRoles.AppAuthor }));
        }
    }
}
