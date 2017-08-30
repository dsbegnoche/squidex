using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Squidex.Infrastructure;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Users
{
    public class UserManagerExtensionsTests
	{
		private readonly UserManager<IUser> userManager;
		private string testEmail = "test@test.com";
		private string testId = "ValidID";
		private string testFirstName = "First";
		private string testLastName = "Last";
		private readonly Mock<IQueryableUserStore<IUser>> store = new Mock<IQueryableUserStore<IUser>>();
		private UserExtenstionsTests.TestUser testUser;
		private readonly Mock<IUserFactory> mockFactory = new Mock<IUserFactory>();

		public UserManagerExtensionsTests()
		{
			var logins = new List<ExternalLogin>
			{
				new ExternalLogin("CivicPlus", "53839115-F8DE-42A2-8911-9C1635D1F99F", "CivicPlus")
			};
			testUser = new UserExtenstionsTests.TestUser(testEmail, testId, false, new List<Claim>(), logins, testFirstName, testLastName);
			store.Setup(u => u.CreateAsync(testUser, new CancellationToken(false))).Returns(Task.FromResult(IdentityResult.Success));
			mockFactory.Setup(u => u.Create(testEmail)).Returns(testUser);
			userManager = new FakeUserManager(store);
		}

		[Fact]
		public void QueryByEmailAsyncTestValidEmailSkip0Take1()
		{
			var userList = userManager.QueryByEmailAsync(testEmail, 1, 0);

			Assert.Equal(1, userList.Result.Count);
			Assert.Equal(testEmail, userList.Result[0].Email);
		}

		[Fact]
		public void QueryByEmailAsyncTestValidEmailSkip1Take10()
		{
			var userList = userManager.QueryByEmailAsync(testEmail, 10, 1);

			Assert.Equal(1, userList.Result.Count);
			Assert.Equal(testEmail, userList.Result[0].Email);
		}

		[Fact]
		public void QueryByEmailAsyncTestValidEmailSkip0Take2()
		{
			var userList = userManager.QueryByEmailAsync(testEmail, 2, 0);

			Assert.Equal(2, userList.Result.Count);
			Assert.Equal(testEmail, userList.Result[0].Email);
			Assert.Equal(testEmail, userList.Result[1].Email);
		}

		[Fact]
		public void QueryByEmailAsyncTestInvalidEmail()
		{
			var userList = userManager.QueryByEmailAsync("BADEMAIL@bad.com");

			Assert.Equal(0, userList.Result.Count);
		}

		[Fact]
		public void CountByEmailAsync()
		{
			Assert.Equal(2, userManager.CountByEmailAsync(testEmail).Result);
		}

		[Fact]
		public void QueryByIdentityServerIdTest()
		{
			Assert.Equal(testUser.Id, userManager.QueryByIdentityServerId("53839115-F8DE-42A2-8911-9C1635D1F99F").Result.Id);
		}

		[Fact]
		public async void CreateAsyncTestNoPassword()
		{
			var createdUser = await userManager.CreateAsync(mockFactory.Object, testEmail, testEmail, null);
			Assert.Equal(testEmail, createdUser.Email);
		}

		[Fact]
		public void CreateAsyncTestNoPasswordException()
		{
			store.Setup(u => u.CreateAsync(testUser, new CancellationToken(false))).Returns(Task.FromResult(IdentityResult.Failed()));
			var ex = Assert.ThrowsAsync<ValidationException>(() => userManager.CreateAsync(mockFactory.Object, testEmail, testEmail, null));
			Assert.NotNull(ex);
			Assert.Equal("Cannot create user.", ex.Result.Message);
		}

		public class FakeUserManager : UserManager<IUser>
		{
			public FakeUserManager(Mock<IQueryableUserStore<IUser>> storeMock)
				: base(storeMock.Object,
					new Mock<IOptions<IdentityOptions>>().Object,
					new Mock<IPasswordHasher<IUser>>().Object,
					new IUserValidator<IUser>[0],
					new IPasswordValidator<IUser>[0],
					new Mock<ILookupNormalizer>().Object,
					new Mock<IdentityErrorDescriber>().Object,
					new Mock<IServiceProvider>().Object,
					new Mock<ILogger<UserManager<IUser>>>().Object)
			{ }

			public override async Task<IdentityResult> AddPasswordAsync(IUser user, string password)
			{
				return IdentityResult.Success;
			}

			public override IQueryable<IUser> Users
			{
				get
				{
					string testEmail = "test@test.com";
					string testFirstName = "First";
					string testLastName = "Last";
					IReadOnlyList<Claim> testClaims = new List<Claim>();
					var testLogins = new List<ExternalLogin>
					{
						new ExternalLogin("CivicPlus", "53839115-F8DE-42A2-8911-9C1635D1F99F", "CivicPlus")
					};
					UserExtenstionsTests.TestUser validUser = new UserExtenstionsTests.TestUser(testEmail, "ValidID", false, testClaims, testLogins, testFirstName, testLastName);
					UserExtenstionsTests.TestUser lockedUser = new UserExtenstionsTests.TestUser(testEmail, "LockedID", true, testClaims, testLogins, testFirstName, testLastName);

					var retVal = new List<IUser> {validUser, lockedUser};

					return retVal.AsQueryable();
				}
			}

			public override string NormalizeKey(string email)
			{
				return email;
			}

			public override async Task<IUser> FindByIdAsync(string userId)
			{
				return Users.FirstOrDefault(u => u.Id == userId);
			}

			public override async Task<IdentityResult> SetLockoutEndDateAsync(IUser user, DateTimeOffset? lockoutEnd)
			{
				return IdentityResult.Success;
			}
		}
	}
	
}
