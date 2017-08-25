// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using Squidex.Domain.Apps.Read.Apps;

namespace Squidex.Domain.Apps.Write.Contents.Commands
{
	public class CopyContent : ContentCommand
	{
		public IAppEntity App { get; set; }

		public Guid CopyFromId { get; set; }

		public CopyContent()
		{
			ContentId = Guid.NewGuid();
		}
	}
}
