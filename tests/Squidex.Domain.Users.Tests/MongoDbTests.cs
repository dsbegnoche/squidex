using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using MongoDB.Driver;
using Moq;
using Squidex.Domain.Users.MongoDb;
using Squidex.Shared.Identity;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Users
{
    public class MongoDbTests
    {
	    private readonly Mock<IMongoDatabase> mockMongoDb = new Mock<IMongoDatabase>();
	    private readonly Mock<IMongoCollection<WrappedIdentityUser>> mockUserCollection = new Mock<IMongoCollection<WrappedIdentityUser>>();
	    private readonly MongoUserStore testUserStore;
	    private readonly MongoRoleStore testRoleStore;
	    private const string testEmail = "test@test.com";
	    private const string testId = "testId";
	    private const string testSecurityStamp = "testSecurityStamp";
	    private const string testUserName = "testUserName";
	    private const string testPhoneNumber = "555-555-5555";
	    private readonly DateTimeOffset? testLockoutDate = DateTimeOffset.MaxValue;
	    private const int testAccessFailedCount = 2;
	    private const string testToken = "testToken";
	    private const string testPasswordHash = "testPasswordHash";
	    private const string testProvider = "testProvider";
	    private const string testKey = "testKey";
	    private readonly UserLoginInfo testLoginInfo;
	    private readonly Claim testClaim;
	    private readonly WrappedIdentityUser testUser;
	    private const string admin = "ADMINISTRATOR";
	    private readonly CancellationToken cancellationToken = new CancellationToken(false);
	    private readonly IQueryable<WrappedIdentityUser> testQueryableUsers;

	    private readonly WrappedIdentityRole testRole;


		public MongoDbTests()
	    {
		    mockMongoDb.Setup(x => x.GetCollection<WrappedIdentityUser>("Identity_Users", null)).Returns(mockUserCollection.Object);
		    Mock<UserStore<WrappedIdentityUser>> mockUserStore = new Mock<UserStore<WrappedIdentityUser>>(mockMongoDb.Object.GetCollection<WrappedIdentityUser>("Identity_Users", null));
		    Mock<RoleStore<WrappedIdentityRole>> mockRoleStore = new Mock<RoleStore<WrappedIdentityRole>>(mockMongoDb.Object.GetCollection<WrappedIdentityUser>("Identity_Roles", null));
		    testUser = new WrappedIdentityUser();
		    testUser.Email = testUser.NormalizedEmail = testEmail;
		    testUser.Id = testId;
		    testUser.SecurityStamp = testSecurityStamp;
		    testUser.PhoneNumber = testPhoneNumber;
		    testUser.PasswordHash = testPasswordHash;
		    testUser.LockoutEnabled = true;
		    testUser.LockoutEndDateUtc = DateTime.MaxValue;
		    testUser.AccessFailedCount = testAccessFailedCount;
		    testUser.TwoFactorEnabled = true;
		    testUser.UserName = testUser.NormalizedUserName = testUserName;
			testClaim = new Claim(SquidexClaimTypes.SquidexDisplayName, testEmail);
			testUser.Claims.Add(new IdentityUserClaim(testClaim));
			testUser.Roles = new List<string> { admin };
		    testLoginInfo = new UserLoginInfo(testProvider, testKey, "");

			List<WrappedIdentityUser> userList = new List<WrappedIdentityUser> { testUser };
			List<UserLoginInfo> userLoginList = new List<UserLoginInfo> { testLoginInfo };
			List<Claim> claimsList = new List<Claim> { testClaim };
			testQueryableUsers = userList.AsQueryable();
		    IList<WrappedIdentityUser> testImmutableUsers = userList.ToImmutableList();
		    IList<string> testUserRoles = testUser.Roles.ToImmutableList();
		    IList<UserLoginInfo> testLoginInfoList = userLoginList.ToImmutableList();
		    IList<Claim> testClaimsList = claimsList.ToImmutableList();

			//Setup the user store
			mockUserStore.Setup(x => x.Users).Returns(testQueryableUsers);
		    mockUserStore.Setup(x => x.FindByIdAsync(testId, cancellationToken)).Returns(Task.FromResult(testUser));
		    mockUserStore.Setup(x => x.FindByEmailAsync(testEmail, cancellationToken)).Returns(Task.FromResult(testUser));
		    mockUserStore.Setup(x => x.FindByLoginAsync(testProvider, testKey, cancellationToken)).Returns(Task.FromResult(testUser));
		    mockUserStore.Setup(x => x.FindByNameAsync(testEmail, cancellationToken)).Returns(Task.FromResult(testUser));
		    mockUserStore.Setup(x => x.GetUsersForClaimAsync(testClaim, cancellationToken)).Returns(Task.FromResult(testImmutableUsers));
		    mockUserStore.Setup(x => x.GetUsersInRoleAsync(admin, cancellationToken)).Returns(Task.FromResult(testImmutableUsers));
		    mockUserStore.Setup(x => x.CreateAsync(testUser, cancellationToken)).Returns(Task.FromResult(IdentityResult.Success));
		    mockUserStore.Setup(x => x.UpdateAsync(testUser, cancellationToken)).Returns(Task.FromResult(IdentityResult.Success));
		    mockUserStore.Setup(x => x.DeleteAsync(testUser, cancellationToken)).Returns(Task.FromResult(IdentityResult.Success));
		    mockUserStore.Setup(x => x.GetUserIdAsync(testUser, cancellationToken)).Returns(Task.FromResult(testUser.Id));
		    mockUserStore.Setup(x => x.GetUserNameAsync(testUser, cancellationToken)).Returns(Task.FromResult(testUser.UserName));
		    mockUserStore.Setup(x => x.GetNormalizedUserNameAsync(testUser, cancellationToken)).Returns(Task.FromResult(testUser.NormalizedUserName));
		    mockUserStore.Setup(x => x.GetRolesAsync(testUser, cancellationToken)).Returns(Task.FromResult(testUserRoles));
		    mockUserStore.Setup(x => x.IsInRoleAsync(testUser, admin, cancellationToken)).Returns(Task.FromResult(true));
		    mockUserStore.Setup(x => x.GetLoginsAsync(testUser, cancellationToken)).Returns(Task.FromResult(testLoginInfoList));
		    mockUserStore.Setup(x => x.GetSecurityStampAsync(testUser, cancellationToken)).Returns(Task.FromResult(testSecurityStamp));
		    mockUserStore.Setup(x => x.GetEmailAsync(testUser, cancellationToken)).Returns(Task.FromResult(testEmail));
		    mockUserStore.Setup(x => x.GetEmailConfirmedAsync(testUser, cancellationToken)).Returns(Task.FromResult(true));
		    mockUserStore.Setup(x => x.GetNormalizedEmailAsync(testUser, cancellationToken)).Returns(Task.FromResult(testEmail));
		    mockUserStore.Setup(x => x.GetClaimsAsync(testUser, cancellationToken)).Returns(Task.FromResult(testClaimsList));
		    mockUserStore.Setup(x => x.GetPhoneNumberAsync(testUser, cancellationToken)).Returns(Task.FromResult(testPhoneNumber));
		    mockUserStore.Setup(x => x.GetPhoneNumberConfirmedAsync(testUser, cancellationToken)).Returns(Task.FromResult(true));
		    mockUserStore.Setup(x => x.GetTwoFactorEnabledAsync(testUser, cancellationToken)).Returns(Task.FromResult(testUser.TwoFactorEnabled));
		    mockUserStore.Setup(x => x.GetLockoutEndDateAsync(testUser, cancellationToken)).Returns(Task.FromResult(testLockoutDate));
		    mockUserStore.Setup(x => x.GetLockoutEnabledAsync(testUser, cancellationToken)).Returns(Task.FromResult(testUser.LockoutEnabled));
		    mockUserStore.Setup(x => x.GetAccessFailedCountAsync(testUser, cancellationToken)).Returns(Task.FromResult(testAccessFailedCount));
		    mockUserStore.Setup(x => x.IncrementAccessFailedCountAsync(testUser, cancellationToken)).Returns(Task.FromResult(testAccessFailedCount + 1));
		    mockUserStore.Setup(x => x.GetTokenAsync(testUser, testProvider, testUserName, cancellationToken)).Returns(Task.FromResult(testToken));

			testUserStore = new MongoUserStore(mockUserStore.Object);

			//Setup the role store
			testRole = new WrappedIdentityRole { Name = admin };
		    mockRoleStore.Setup(y => y.FindByIdAsync(testId, cancellationToken)).Returns(Task.FromResult(testRole));
		    mockRoleStore.Setup(y => y.FindByIdAsync(testId, cancellationToken)).Returns(Task.FromResult(testRole));
		    mockRoleStore.Setup(y => y.FindByNameAsync(admin, cancellationToken)).Returns(Task.FromResult(testRole));
			mockRoleStore.Setup(y => y.CreateAsync(testRole, cancellationToken)).Returns(Task.FromResult(IdentityResult.Success));
		    mockRoleStore.Setup(y => y.UpdateAsync(testRole, cancellationToken)).Returns(Task.FromResult(IdentityResult.Success));
		    mockRoleStore.Setup(y => y.DeleteAsync(testRole, cancellationToken)).Returns(Task.FromResult(IdentityResult.Success));
		    mockRoleStore.Setup(y => y.GetRoleIdAsync(testRole, cancellationToken)).Returns(Task.FromResult(testId));
		    mockRoleStore.Setup(y => y.GetRoleNameAsync(testRole, cancellationToken)).Returns(Task.FromResult(admin));
		    mockRoleStore.Setup(y => y.GetNormalizedRoleNameAsync(testRole, cancellationToken)).Returns(Task.FromResult(admin));

			testRoleStore = new MongoRoleStore(mockRoleStore.Object);
		}

		#region MongoUserStoreTests
		[Fact]
	    public void UsersListTest()
	    {
		    IQueryable<IUser> testUsersListReturned = testUserStore.Users;
		    Assert.Equal(testQueryableUsers, testUsersListReturned);
	    }

		[Fact]
	    public void CreateUserTest()
	    {
		    IUser user = testUserStore.Create(testEmail);
		    Assert.Equal(testEmail, user.Email);
	    }

	    [Fact]
	    public void FindByIdAsyncTest()
	    {
		    IUser user = testUserStore.FindByIdAsync(testId).Result;
			Assert.Equal(testId, user.Id);

		    user = testUserStore.FindByIdAsync(testId, cancellationToken).Result;
		    Assert.Equal(testId, user.Id);
		}

	    [Fact]
	    public void FindByEmailAsyncTest()
		{
			IUser user = testUserStore.FindByEmailAsync(testEmail, cancellationToken).Result;
			Assert.Equal(testEmail, user.Email);
		}

	    [Fact]
	    public void FindByLoginAsyncTest()
	    {
		    IUser user = testUserStore.FindByLoginAsync(testProvider, testKey, cancellationToken).Result;
		    Assert.Equal(testEmail, user.Email);
		}

	    [Fact]
	    public void FindByNameAsyncTest()
	    {
		    IUser user = testUserStore.FindByNameAsync(testEmail, cancellationToken).Result;
		    Assert.Equal(testEmail, user.Email);
	    }

	    [Fact]
	    public void GetUsersForClaimAsyncTest()
	    {
		    IList<IUser> testUserList = testUserStore.GetUsersForClaimAsync(testClaim, cancellationToken).Result;
			Assert.Equal(1, testUserList.Count);
			Assert.Equal(testUser, testUserList[0]);
		}

	    [Fact]
	    public void GetUsersInRoleAsyncTest()
	    {
		    IList<IUser> testUserList = testUserStore.GetUsersInRoleAsync(admin, cancellationToken).Result;
		    Assert.Equal(1, testUserList.Count);
		    Assert.Equal(testUser, testUserList[0]);
	    }

		[Fact]
	    public void CreateAsyncTest()
	    {
		    Assert.Equal(IdentityResult.Success, testUserStore.CreateAsync(testUser, cancellationToken).Result);
		}

	    [Fact]
	    public void UpdateAsyncTest()
	    {
		    Assert.Equal(IdentityResult.Success, testUserStore.UpdateAsync(testUser, cancellationToken).Result);
		}

	    [Fact]
	    public void DeleteAsyncTest()
	    {
		    Assert.Equal(IdentityResult.Success, testUserStore.DeleteAsync(testUser, cancellationToken).Result);
	    }

	    [Fact]
	    public void GetUserIdAsyncTest()
	    {
		    string returnId = testUserStore.GetUserIdAsync(testUser, cancellationToken).Result;
			Assert.Equal(testId, returnId);
		}

	    [Fact]
	    public void GetUserNameAsyncTest()
	    {
		    string returnName = testUserStore.GetUserNameAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testUserName, returnName);
		}

	    [Fact]
	    public void GetNormalizedUserNameAsyncTest()
	    {
		    string returnName = testUserStore.GetNormalizedUserNameAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testUserName, returnName);
		}

	    [Fact]
	    public void GetRolesAsyncTest()
	    {
		    IList<string> roles = testUserStore.GetRolesAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testUser.Roles, roles);
		}

	    [Fact]
	    public void IsInRoleAsyncTest()
	    {
			bool result = testUserStore.IsInRoleAsync(testUser, admin, cancellationToken).Result;
		    Assert.True(result);
		}

	    [Fact]
	    public void GetLoginsAsyncTest()
	    {
		    IList<UserLoginInfo> result = testUserStore.GetLoginsAsync(testUser, cancellationToken).Result;
		    Assert.Equal(1, result.Count);
		    Assert.Equal(testLoginInfo, result[0]);
		}

	    [Fact]
	    public void GetSecurityStampAsyncTest()
	    {
		    string retVal = testUserStore.GetSecurityStampAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testSecurityStamp, retVal);
		}

	    [Fact]
	    public void GetEmailAsyncTest()
	    {
		    string retVal = testUserStore.GetEmailAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testEmail, retVal);
		}

	    [Fact]
	    public void GetEmailConfirmedAsyncTest()
	    {
		    bool retVal = testUserStore.GetEmailConfirmedAsync(testUser, cancellationToken).Result;
		    Assert.True(retVal);
		}

	    [Fact]
	    public void GetNormalizedEmailAsyncTest()
	    {
		    string retVal = testUserStore.GetNormalizedEmailAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testEmail, retVal);
		}

	    [Fact]
	    public void GetClaimsAsyncTest()
	    {
		    IList<Claim> retVal = testUserStore.GetClaimsAsync(testUser, cancellationToken).Result;
		    Assert.Equal(1, retVal.Count);
		    Assert.Equal(testClaim, retVal[0]);
		}

	    [Fact]
	    public void GetPhoneNumberAsyncTest()
	    {
		    string retVal = testUserStore.GetPhoneNumberAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testPhoneNumber, retVal);
		}

		[Fact]
		public void GetPhoneNumberConfirmedAsyncTest()
		{
			bool retVal = testUserStore.GetPhoneNumberConfirmedAsync(testUser, cancellationToken).Result;
			Assert.True(retVal);
		}

		[Fact]
		public void GetTwoFactorEnabledAsyncTest()
		{
			bool retVal = testUserStore.GetTwoFactorEnabledAsync(testUser, cancellationToken).Result;
			Assert.True(retVal);
		}

		[Fact]
		public void GetLockoutEndDateAsyncTest()
		{
			DateTimeOffset? retVal = testUserStore.GetLockoutEndDateAsync(testUser, cancellationToken).Result;
			Assert.Equal(testUser.LockoutEndDateUtc.Value, retVal.Value.DateTime);
		}

		[Fact]
		public void GetLockoutEnabledAsyncTest()
		{
			bool retVal = testUserStore.GetLockoutEnabledAsync(testUser, cancellationToken).Result;
			Assert.True(retVal);
		}

	    [Fact]
	    public void GetAccessFailedCountAsyncTest()
	    {
		    int retVal = testUserStore.GetAccessFailedCountAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testAccessFailedCount, retVal);
	    }

	    [Fact]
	    public void IncrementAccessFailedCountAsyncTest()
	    {
		    int retVal = testUserStore.IncrementAccessFailedCountAsync(testUser, cancellationToken).Result;
		    Assert.Equal(testAccessFailedCount + 1, retVal);
		}

	    [Fact]
	    public void GetTokenAsyncTest()
	    {
		    string retVal = testUserStore.GetTokenAsync(testUser, testProvider, testUserName, cancellationToken).Result;
		    Assert.Equal(testToken, retVal);
		}

	    [Fact]
	    public void HasPasswordAsyncTest()
	    {
		    bool retVal = testUserStore.HasPasswordAsync(testUser, cancellationToken).Result;
		    Assert.True(retVal);
		}
#endregion

		#region WrappedIdentityUserTests

		[Fact]
	    public void UserIsLockedTest()
	    {
		    bool retVal = testUser.IsLocked;
		    Assert.True(retVal);
		}

	    [Fact]
	    public void UserSetClaimTest()
	    {
		    IdentityUserClaim expectedClaim = new IdentityUserClaim { Type = SquidexClaimTypes.SquidexDisplayName, Value = "testDisplayClaim" };
			testUser.SetClaim(SquidexClaimTypes.SquidexDisplayName, "testDisplayClaim");
		    Assert.Equal(1, testUser.Claims.Count);
		    Assert.Equal(expectedClaim.Type, testUser.Claims[0].Type);
		    Assert.Equal(expectedClaim.Value, testUser.Claims[0].Value);
	    }

	    [Fact]
	    public void UserUpdateEmailTest()
	    {
		    string updatedEmail = "updated@test.com";
		    testUser.UpdateEmail(updatedEmail);
			Assert.Equal(updatedEmail, testUser.Email);
	    }
		#endregion

		#region MongoRoleStore

	    [Fact]
	    public void CreateRoleTest()
	    {
		    IRole role = testRoleStore.Create(admin);
		    Assert.Equal(admin, role.Name);
	    }

	    [Fact]
	    public void FindRoleByIdAsyncTest()
	    {
		    IRole role = testRoleStore.FindByIdAsync(testId, cancellationToken).Result;
		    Assert.Equal(admin, role.Name);
	    }

	    [Fact]
	    public void FindRoleByNameAsyncTest()
	    {
		    IRole role = testRoleStore.FindByNameAsync(admin, cancellationToken).Result;
		    Assert.Equal(admin, role.Name);
		}

	    [Fact]
	    public void CreateRoleAsyncTest()
	    {
		    Assert.Equal(IdentityResult.Success, testRoleStore.CreateAsync(testRole, cancellationToken).Result);
	    }

	    [Fact]
	    public void UpdateRoleAsyncTest()
	    {
		    Assert.Equal(IdentityResult.Success, testRoleStore.UpdateAsync(testRole, cancellationToken).Result);
	    }

	    [Fact]
	    public void DeleteRoleAsyncTest()
	    {
		    Assert.Equal(IdentityResult.Success, testRoleStore.DeleteAsync(testRole, cancellationToken).Result);
	    }

	    [Fact]
	    public void GetRoleIdAsyncTest()
	    {
		    string retVal = testRoleStore.GetRoleIdAsync(testRole, cancellationToken).Result;
			Assert.Equal(testId, retVal);
		}

	    [Fact]
	    public void GetRoleNameAsyncTest()
	    {
		    string retVal = testRoleStore.GetRoleNameAsync(testRole, cancellationToken).Result;
		    Assert.Equal(admin, retVal);
		}

	    [Fact]
	    public void GetNormalizedRoleNameAsyncTest()
	    {
		    string retVal = testRoleStore.GetNormalizedRoleNameAsync(testRole, cancellationToken).Result;
		    Assert.Equal(admin, retVal);
	    }
		#endregion
	}
}

