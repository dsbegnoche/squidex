// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;

namespace Squidex.Controllers.UI.Account
{
	public class ChangeIndentity
	{
		/// <summary>Type of notification action.</summary>
		public string Action { get; set; }

		/// <summary>First name of user changed.</summary>
		public string FirstName { get; set; }

		/// <summary>LAst name of user changed.</summary>
		public string LastName { get; set; }

		/// <summary>Email of user changed.</summary>
		public string Email { get; set; }

		/// <summary>Identity Sever Id of user changed.</summary>
		public Guid Id { get; set; }

		/// <summary>Utc date time when the change was performed on Identity Server.</summary>
		public DateTimeOffset DateTimeOffsetUtc { get; set; }
	}
}
