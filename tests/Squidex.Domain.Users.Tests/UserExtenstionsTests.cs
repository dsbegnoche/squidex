using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Shared.Users;
using Xunit;

namespace Squidex.Domain.Users
{
    public class UserExtenstionsTests
    {
		private readonly TestUser testUser = new TestUser();
	    private string testEmail = "test@test.com";
	    private string testPictureUrl = "www.testpictureurl.com";
	    private string testId = "TESTID";
		private readonly IReadOnlyList<Claim> testClaims = new List<Claim>();
		private readonly IReadOnlyList<ExternalLogin> testLogins = new List<ExternalLogin>();

		public UserExtenstionsTests()
		{
			testUser.Email = testEmail;
			testUser.IsLocked = false;
			testUser.Id = testId;
			testUser.NormalizedEmail = testEmail;
			testUser.Claims = testClaims;
			testUser.Logins = testLogins;
		}

	    [Fact]
	    public void VerifyUserGets()
	    {
		    Assert.Equal(testEmail, testUser.Email);
		    Assert.Equal(testEmail, testUser.NormalizedEmail);
		    Assert.Equal(false, testUser.IsLocked);
		    Assert.Equal(testId, testUser.Id);
		    Assert.Equal(testId, testUser.Id);
		    Assert.Equal(testClaims, testUser.Claims);
		    Assert.Equal(testLogins, testUser.Logins);
	    }
		
		[Fact]
	    public void SetDisplayNameTest()
		{
			testUser.SetDisplayName(testUser.Email);
			Assert.Equal(testEmail, testUser.DisplayName());
		}

	    [Fact]
	    public void SetPictureUrlTest()
	    {
		    testUser.SetPictureUrl(testPictureUrl);
			Assert.Equal(testPictureUrl, testUser.PictureUrl());
		}

	    [Fact]
	    public void SetPictureUrlToStoreTest()
	    {
		    testUser.SetPictureUrlToStore();
			Assert.True(testUser.IsPictureUrlStored());
		}

	    [Fact]
	    public void SetPictureUrlFromGravatarTest()
	    {
		    testUser.SetPictureUrlFromGravatar(testUser.Email);
			Assert.True(testUser.PictureUrl().StartsWith("https://www.gravatar.com/avatar/"));
		}

	    [Fact]
	    public void PictureNormalizedUrlTestNoQuestionMarkInUrl()
		{
			testUser.SetPictureUrlFromGravatar(testUser.Email);
			Assert.True(testUser.PictureNormalizedUrl().EndsWith("?d=404"));
		}

	    [Fact]
	    public void PictureNormalizedUrlTestQuestionMarkInUrl()
	    {
		    testUser.SetPictureUrl($"http://{testPictureUrl}/gravatar?test=Test");
		    Assert.True(testUser.PictureNormalizedUrl().EndsWith("&d=404"));
	    }

		public class TestUser : IUser
	    {
		    public TestUser()
		    {
			    ClaimsList = new List<IdentityUserClaim>();
		    }

		    public void AddClaim(Claim claim)
		    {
		    }

		    public bool IsLocked { get; set; }

		    public string Id { get; set; }

		    public string Email { get; set; }

		    public string NormalizedEmail { get; set; }

		    public IReadOnlyList<Claim> Claims { get; set; }

		    public List<IdentityUserClaim> ClaimsList { get; set; }

			public IReadOnlyList<ExternalLogin> Logins { get; set; }

		    public void UpdateEmail(string email){}

		    public void SetClaim(string type, string value)
		    {
			    ClaimsList.RemoveAll(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
			    ClaimsList.Add(new IdentityUserClaim { Type = type, Value = value });
			    this.Claims = this.ClaimsList.Select(x => new Claim(x.Type, x.Value)).ToList();
			}
		}
	}
}
