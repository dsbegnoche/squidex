using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CivicPlusIdentityServer.SDK.NetCore.Entities;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using RestSharp;
using Squidex.Controllers.Toolbar.Models;
using Squidex.Pipeline;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Squidex.Shared.Users;
using Squidex.Config;
using Squidex.Config.Identity;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Controllers.Toolbar
{
	[SwaggerIgnore]
	public class ToolbarController : Controller
	{
		private readonly UserManager<IUser> userManager;
		private readonly CivicPlusIdentityServer.SDK.NetCore.Base.IActions civicplusIdentityServerSdk;
		private readonly IOptions<MyIdentityOptions> identityOptions;

		public ToolbarController(UserManager<IUser> userManager,
			CivicPlusIdentityServer.SDK.NetCore.Base.IActions civicplusIdentityServerSdk,
			IOptions<MyIdentityOptions> identityOptions)
		{
			this.userManager = userManager;
			this.civicplusIdentityServerSdk = civicplusIdentityServerSdk;
			this.identityOptions = identityOptions;
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
	    [ProducesResponseType(typeof(CpProductsDto[]), 200)]
	    [ApiCosts(1)]
		public async Task<IActionResult> GetProductDropdown()
		{
			var user = await userManager.GetUserAsync(User);
			var emailAddress = user.Email;
			var accessToken = user.GetTokenValue(Constants.CivicPlusAuthenticationScheme, "access_token");

			List<ClientInfo> products = civicplusIdentityServerSdk.GetClientList(accessToken, emailAddress);

			var response = products.Select(p =>
			{
				var dto = SimpleMapper.Map(p, new CpProductsDto());

				return dto;
			}).ToList();


			return Ok(response);
		}

		/// <summary>
		/// Get CP Help Links.
		/// </summary>
		/// <returns>
		/// 200 => CP Help Links returned.
		/// </returns>
		/// <remarks>
		/// You will retrieve all CP Help Links from the CivicPlusPlatform.
		/// </remarks>
		[HttpGet]
		[Route("cptoolbar/helplinks/")]
		[ProducesResponseType(typeof(CpHelpLinksDto[]), 200)]
		[ApiCosts(1)]
		public IActionResult GetHelpLinks()
		{
			var client = new RestClient($"{identityOptions.Value.CivicPlusPlatformBaseUrl}/api/helplinks");
			var request = new RestRequest(Method.GET);
			IRestResponse<List<CpHelpLinksDto>> response = new RestResponse<List<CpHelpLinksDto>>();

			Task.Run(async () =>
			{
				response = await GetResponseContentAsync<List<CpHelpLinksDto>>(client, request);
			}).Wait();

			if (response.StatusCode == HttpStatusCode.OK)
			{
				List<CpHelpLinksDto> helpLinks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<CpHelpLinksDto>>(response.Content);
				return Ok(helpLinks);
			}
			return BadRequest();
		}

		private static Task<IRestResponse<T>> GetResponseContentAsync<T>(IRestClient restClient, IRestRequest theRequest) where T : new()
		{
			var complete = new TaskCompletionSource<IRestResponse<T>>();
			restClient.ExecuteAsync<T>(theRequest, (response, handler) =>
			{
				complete.SetResult(response);
			});
			return complete.Task;
		}

		/// <summary>
		/// Get CP Reset Password link.
		/// </summary>
		/// <returns>
		/// 200 => CP Reset Password link returned.
		/// </returns>
		/// <remarks>
		/// You will retrieve CP Reset Password link.
		/// </remarks>
		[HttpGet]
		[Route("cptoolbar/reset-password/")]
		[ProducesResponseType(typeof(CpHelpLinksDto[]), 200)]
		[ApiCosts(1)]
		public IActionResult ResetPassword()
		{
			var resetPasswordUrl = $"{identityOptions.Value.CivicPlusPlatformBaseUrl}/ResetPassword?redirectUrl=";
			var response = new List<CpHelpLinksDto>();
			response.Add(new CpHelpLinksDto
			{
				Id = 0,
				Title = "ResetPasswordLink",
				Url = resetPasswordUrl
			});


			return Ok(response);
		}
	}
}
