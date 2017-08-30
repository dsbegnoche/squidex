// ==========================================================================
//  CreateApp.cs
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Domain.Apps.Write.Apps.Commands
{
    public sealed class DeleteApp : AppAggregateCommand
	{

		public DeleteApp(string name, Guid id)
		{
			AppId = new NamedId<Guid>(id, name);
		}
	}
}