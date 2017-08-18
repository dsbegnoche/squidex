namespace Squidex.Controllers.Toolbar.Models
{
    public sealed class CpProductsDto
	{        /// <summary>
		/// The name of the product.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// The id of the product.
		/// </summary>
		public int Id { get; set; }
		
		/// <summary>
		/// The description of the product.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The logo URI of the product.
		/// </summary>
		public string LogoUri { get; set; }

		/// <summary>
		/// The logout URI of the product.
		/// </summary>
		public string LogoutUri { get; set; }

		/// <summary>
		/// The URI of the product.
		/// </summary>
		public string ProductUri { get; set; }

		/// <summary>
		/// Is the product hidden by user
		/// </summary>
		public bool IsProductHidden { get; set; }
	}
}
