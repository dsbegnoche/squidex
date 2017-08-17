using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Toolbar.Models
{
    public sealed class CpProductsDto
	{        /// <summary>
		/// The name of the app.
		/// </summary>
		[Required]
		[RegularExpression("^[a-z0-9]+(\\-[a-z0-9]+)*$")]
		public string Name { get; set; }

		/// <summary>
		/// The version of the app.
		/// </summary>
		public long Version { get; set; }

		/// <summary>
		/// The id of the app.
		/// </summary>
		public Guid Id { get; set; }
	}
}
