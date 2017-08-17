using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using RestSharp;
using Squidex.Controllers.Toolbar.Models;
using Squidex.Pipeline;
using Microsoft.AspNetCore.Identity;
using Squidex.Shared.Users;
using Squidex.Config;

namespace Squidex.Controllers.Toolbar
{
	[SwaggerIgnore]
	public class ToolbarController : Controller
	{
		private readonly UserManager<IUser> userManager;
		private readonly CivicPlusIdentityServer.SDK.Base.IActions civicplusIdentityServerSdk;

		public ToolbarController(UserManager<IUser> userManager,
			CivicPlusIdentityServer.SDK.Base.IActions civicplusIdentityServerSdk)
		{
			this.userManager = userManager;
			this.civicplusIdentityServerSdk = civicplusIdentityServerSdk;
		}

		/// <summary>
		/// Get your CP Products.
		/// </summary>
		/// <returns>
		/// 200 => CP Products returned.
		/// </returns>
		/// <remarks>
		/// You can only retrieve the list of CP Products when you are authenticated as a user (OpenID implicit flow).
		/// You will retrieve all CP Products, where you have access to in CivicPlusPlatform.
		/// </remarks>
		[HttpGet]
	    [Route("cptoolbar/products/")]
	    [ProducesResponseType(typeof(CpProducts[]), 200)]
	    [ApiCosts(1)]
		public async Task<IActionResult> GetProductDropdown()
		{
			var user = await userManager.GetUserAsync(User);
			var emailAddress = user.Email;
			var accessToken = user.GetTokenValue(Constants.CivicPlusAuthenticationScheme, "access_token");

			var test = civicplusIdentityServerSdk.GetClientList(accessToken, emailAddress);


			return Ok();
		}

    }
}
