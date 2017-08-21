// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using CivicPlusIdentityServer.SDK.NetCore.Entities;
using CivicPlusIdentityServer.SDK.NetCore.Base;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Config;
using Squidex.Config.Identity;
using Squidex.Controllers.UI.Account;
using Squidex.Domain.Users;
using Squidex.Domain.Users.MongoDb;
using Squidex.Infrastructure.Log;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Tests.Controllers.UI.Account
{
	public class AccountControllerTests
	{
		private readonly Mock<Domain.Users.Base.ISignInManager<IUser>> signInManager;
		private readonly Mock<UserManager<IUser>> userManager;
		private readonly Mock<IUserFactory> userFactory;
		private readonly Mock<IOptions<MyIdentityOptions>> identityOptions;
		private readonly Mock<IOptions<MyUrlsOptions>> urlOptions;
		private readonly Mock<ISemanticLog> log;
		private readonly Mock<IIdentityServerInteractionService> interactions;
		private readonly Mock<IActions> civicplusIdentityServerSdk;
		private readonly Mock<HttpContext> httpContext;
		private readonly HttpContextAccessor httpContextAccessor;
		private readonly Mock<IUserClaimsPrincipalFactory<IUser>> userClaimsPrincipalFactory;
		private readonly Mock<IUserStore<IUser>> userStore;

		private readonly Squidex.Controllers.UI.Account.AccountController systemUnderTest;

		public AccountControllerTests()
		{
			httpContext = new Mock<HttpContext>();
			httpContext.Setup(x => x.User.Identity.IsAuthenticated).Returns(true);
			httpContextAccessor = new HttpContextAccessor() { HttpContext = httpContext.Object };
			userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IUser>>();
			userStore = new Mock<IUserStore<IUser>>();

			userManager = new Mock<UserManager<IUser>>(userStore.Object, null, null, null, null, null, null, null, null);
			signInManager = new Mock<Domain.Users.Base.ISignInManager<IUser>>();
			userFactory = new Mock<IUserFactory>();
			identityOptions = new Mock<IOptions<MyIdentityOptions>>();
			urlOptions = new Mock<IOptions<MyUrlsOptions>>();
			log = new Mock<ISemanticLog>();
			interactions = new Mock<IIdentityServerInteractionService>();
			civicplusIdentityServerSdk = new Mock<IActions>();

			systemUnderTest = new AccountController(signInManager.Object,
				userManager.Object,
				userFactory.Object,
				identityOptions.Object,
				urlOptions.Object,
				log.Object,
				interactions.Object,
				civicplusIdentityServerSdk.Object);

			systemUnderTest.Url = new Mock<IUrlHelper>().Object;
			systemUnderTest.ControllerContext = new ControllerContext(new ActionContext(httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
		}

		[Fact]
		public void LoginTest()
		{
			//Arrange

			//Act
			IActionResult result = systemUnderTest.Login();

			//Assert
			Assert.IsType<ChallengeResult>(result);
		}

		[Fact]
		public async Task ExternalCallbackTest()
		{
			//Arrange
			ClaimsPrincipal claimsPrincipal = new ClaimsPrincipal();
			string displayName = "Testing";
			ExternalLoginInfo info = new ExternalLoginInfo(claimsPrincipal, Constants.CivicPlusAuthenticationScheme, Constants.CivicPlusAuthenticationScheme, displayName);

			Microsoft.AspNetCore.Identity.SignInResult signInResult =
				Microsoft.AspNetCore.Identity.SignInResult.Success;

			signInManager.Setup(x => x.GetExternalLoginInfoWithDisplayNameAsync(It.IsAny<string>()))
				.ReturnsAsync(info);

			signInManager.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
				.ReturnsAsync(signInResult);

			//Act
			IActionResult result = await systemUnderTest.ExternalCallback();

			//Assert
			signInManager.Verify(x => x.UpdateExternalAuthenticationTokensAsync(info), Times.Once);
		}

		[Fact]
		public async Task LogoutTest()
		{
			//Arrange
			interactions.Setup(x => x.GetLogoutContextAsync(It.IsAny<string>()))
				.ReturnsAsync(new LogoutRequest("http://localhost:5000", new LogoutMessage()
				{
					PostLogoutRedirectUri = "http://localhost:5000"
				}));

			userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(new WrappedIdentityUser()
				{
					Tokens = new List<IdentityUserToken>()
					{
						new IdentityUserToken()
						{
							LoginProvider = "CivicPlus",
							Name = "id_token",
							Value = "tokenValue"
						}
					}
				});

			civicplusIdentityServerSdk.Setup(x => x.GetWellKnownConfiguration())
				.Returns(new Configuration()
				{
					EndSessionEndpoint = "https://account.cpdv.ninja/identity/endsession"
				});

			//Act
			IActionResult result = await systemUnderTest.Logout("logoutId");

			//Assert
			interactions.Verify(x => x.GetLogoutContextAsync(It.IsAny<string>()), Times.Once);
			signInManager.Verify(x => x.SignOutAsync(), Times.Once);
			userManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);

			Assert.IsType<RedirectResult>(result);
			Assert.Equal("https://account.cpdv.ninja/identity/endsession?post_logout_redirect_uri=http://localhost:5000&id_token_hint=tokenValue", ((RedirectResult)result).Url);
		}
	}
}
