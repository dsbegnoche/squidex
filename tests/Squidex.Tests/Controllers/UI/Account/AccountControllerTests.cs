// ==========================================================================
//  CivicPlus implementation of Squidex Headless CMS
// ==========================================================================

namespace Squidex.Tests.Controllers.UI.Account
{
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using CivicPlusIdentityServer.SDK.NetCore.Base;
    using CivicPlusIdentityServer.SDK.NetCore.Entities;
    using IdentityServer4.Models;
    using IdentityServer4.Services;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Identity;
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

    public class ContentsControllerTests
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

        public ContentsControllerTests()
        {
            this.httpContext = new Mock<HttpContext>();
            this.httpContext.Setup(x => x.User.Identity.IsAuthenticated).Returns(true);
            this.httpContextAccessor = new HttpContextAccessor() { HttpContext = this.httpContext.Object };
            this.userClaimsPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<IUser>>();
            this.userStore = new Mock<IUserStore<IUser>>();

            this.userManager = new Mock<UserManager<IUser>>(this.userStore.Object, null, null, null, null, null, null, null, null);
            this.signInManager = new Mock<Domain.Users.Base.ISignInManager<IUser>>();
            this.userFactory = new Mock<IUserFactory>();
            this.identityOptions = new Mock<IOptions<MyIdentityOptions>>();
            this.urlOptions = new Mock<IOptions<MyUrlsOptions>>();
            this.log = new Mock<ISemanticLog>();
            this.interactions = new Mock<IIdentityServerInteractionService>();
            this.civicplusIdentityServerSdk = new Mock<IActions>();

            this.systemUnderTest = new AccountController(
                this.signInManager.Object,
                this.userManager.Object,
                this.userFactory.Object,
                this.identityOptions.Object,
                this.urlOptions.Object,
                this.log.Object,
                this.interactions.Object,
                this.civicplusIdentityServerSdk.Object);

            this.systemUnderTest.Url = new Mock<IUrlHelper>().Object;
            this.systemUnderTest.ControllerContext = new ControllerContext(new ActionContext(this.httpContext.Object, new RouteData(), new ControllerActionDescriptor()));
        }

        [Fact]
        public void LoginUserIsAuthenticatedTest()
        {
            // Arrange
            this.httpContext.Setup(x => x.User.Identity.IsAuthenticated).Returns(true);

            // Act
            var result = this.systemUnderTest.Login();

            // Assert
            Assert.IsType<RedirectResult>(result);
        }

        [Fact]
        public void LoginUserIsNotAuthenticatedTest()
        {
            // Arrange
            this.httpContext.Setup(x => x.User.Identity.IsAuthenticated).Returns(false);

            // Act
            var result = this.systemUnderTest.Login();

            // Assert
            Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public async Task ExternalCallbackTest()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal();
            const string displayName = "Testing";
            var info = new ExternalLoginInfo(claimsPrincipal, Constants.CivicPlusAuthenticationScheme, Constants.CivicPlusAuthenticationScheme, displayName);

            var signInResult = Microsoft.AspNetCore.Identity.SignInResult.Success;

            this.signInManager.Setup(x => x.GetExternalLoginInfoWithDisplayNameAsync(It.IsAny<string>()))
                .ReturnsAsync(info);

            this.signInManager.Setup(x => x.ExternalLoginSignInAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(signInResult);

            // Act
           await this.systemUnderTest.ExternalCallback();

            // Assert
            this.signInManager.Verify(x => x.UpdateExternalAuthenticationTokensAsync(info), Times.Once);
        }

        [Fact]
        public async Task LogoutTest()
        {
            // Arrange
            this.interactions.Setup(x => x.GetLogoutContextAsync(It.IsAny<string>()))
                .ReturnsAsync(new LogoutRequest("http://localhost:5000", new LogoutMessage { PostLogoutRedirectUri = "http://localhost:5000" }));

            this.userManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(new MongoUser
                {
                    Tokens = new List<MongoUserToken>
                    {
                        new MongoUserToken()
                        {
                            LoginProvider = Constants.CivicPlusAuthenticationScheme,
                            Name = "id_token",
                            Value = "tokenValue"
                        }
                    }
                });

            this.civicplusIdentityServerSdk.Setup(x => x.GetWellKnownConfiguration())
                .Returns(new Configuration()
                {
                    EndSessionEndpoint = "https://account.cpdv.ninja/identity/endsession"
                });

            // Act
            var result = await this.systemUnderTest.Logout("logoutId");

            // Assert
            this.interactions.Verify(x => x.GetLogoutContextAsync(It.IsAny<string>()), Times.Once);
            this.signInManager.Verify(x => x.SignOutAsync(), Times.Once);
            this.userManager.Verify(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()), Times.Once);

            Assert.IsType<RedirectResult>(result);
            Assert.Equal("https://account.cpdv.ninja/identity/endsession?post_logout_redirect_uri=http://localhost:5000&id_token_hint=tokenValue", ((RedirectResult)result).Url);
        }
    }
}
