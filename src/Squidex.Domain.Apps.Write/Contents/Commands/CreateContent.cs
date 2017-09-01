// ==========================================================================
//  CreateContent.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using System;

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
    public sealed class CreateContent : ContentDataCommand
    {
		public Status Status { get; set; }

        public CreateContent()
        {
            ContentId = Guid.NewGuid();
        }
    }
}
