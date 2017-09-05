using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using IdentityServer4.Endpoints;
using Microsoft.AspNetCore.Identity.MongoDB;
using Squidex.Shared.Identity;
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
        private string testFirstName = "First";
        private string testLastName = "Last";
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
            testUser.FirstName = testFirstName;
            testUser.LastName = testLastName;
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
            Assert.Equal(testFirstName, testUser.FirstName);
            Assert.Equal(testLastName, testUser.LastName);
        }

        [Fact]
        public void SetDisplayNameTest()
        {
            testUser.SetDisplayName(testUser.Email);
            Assert.Equal(testEmail, testUser.DisplayName());
        }

        [Fact]
        public void SetFirstNameTest()
        {
            testUser.SetFirstName(testUser.FirstName);
            Assert.Equal(testFirstName, testUser.FirstName());
        }

        [Fact]
        public void SetLastNameTest()
        {
            testUser.SetLastName(testUser.LastName);
            Assert.Equal(testLastName, testUser.LastName());
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

        [Fact]
        public void NewExternalLogin()
        {
            string testProvider = "testProvider";
            string testKey = "testKey";
            string testDisplayName = "testDisplayName";

            ExternalLogin testLogin = new ExternalLogin(testProvider, testKey, testDisplayName);
            Assert.Equal(testProvider, testLogin.LoginProvider);
            Assert.Equal(testKey, testLogin.ProviderKey);
            Assert.Equal(testDisplayName, testLogin.ProviderDisplayName);
        }

        [Fact]
        public void NewExternalLoginEmptyDisplayName()
        {
            string testProvider = "testProvider";
            string testKey = "testKey";
            string testDisplayName = "";

            ExternalLogin testLogin = new ExternalLogin(testProvider, testKey, testDisplayName);
            Assert.Equal(testProvider, testLogin.LoginProvider);
            Assert.Equal(testKey, testLogin.ProviderKey);
            Assert.Equal(testProvider, testLogin.ProviderDisplayName);
        }

        [Fact]
        public void TestUserRoles()
        {
            string admin = "administrator";
            string appOwner = "app:owner";
            string appEditor = "app:editor";
            string appReader = "app:reader";
            string appDeveloper = "app:dev";
            string appAuthor = "app:author";

            Assert.Equal(admin, SquidexRoles.Administrator);
            Assert.Equal(appOwner, SquidexRoles.AppOwner);
            Assert.Equal(appEditor, SquidexRoles.AppEditor);
            Assert.Equal(appReader, SquidexRoles.AppReader);
            Assert.Equal(appDeveloper, SquidexRoles.AppDeveloper);
            Assert.Equal(appAuthor, SquidexRoles.AppAuthor);
        }

        public class TestUser : IUser
        {
            public TestUser()
            {
                ClaimsList = new List<IdentityUserClaim>();
            }

            public TestUser(string email, string id, bool isLocked, IReadOnlyList<Claim> claims,
                IReadOnlyList<ExternalLogin> logins, string firstName, string lastName, List<string> roles)
            {
                this.Email = this.NormalizedEmail = email;
                this.Id = id;
                this.IsLocked = isLocked;
                this.Claims = claims;
                this.Logins = logins;
                ClaimsList = new List<IdentityUserClaim>();
                this.FirstName = firstName;
                this.LastName = lastName;
                this._Roles = roles;
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

            private List<string> _Roles { get; }

            public IReadOnlyList<string> Roles => _Roles;

            public void UpdateEmail(string email)
            {
                this.Email = email;
            }

            public void SetClaim(string type, string value)
            {
                ClaimsList.RemoveAll(x => string.Equals(x.Type, type, StringComparison.OrdinalIgnoreCase));
                ClaimsList.Add(new IdentityUserClaim { Type = type, Value = value });
                this.Claims = this.ClaimsList.Select(x => new Claim(x.Type, x.Value)).ToList();
            }

            private string tokenValue = "tokenValue";

            public string GetTokenValue(string loginProvider, string name)
            {
                return tokenValue;
            }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public void AddRole(string role)
            {
                if (!InRole(role))
                {
                    _Roles.Add(role);
                }
            }

            public void RemoveRole(string role)
            {
                if (InRole(role))
                {
                    _Roles.Remove(role);
                }
            }

            public bool InRole(string role)
            {
                return _Roles.Exists(x => string.Equals(x, role, StringComparison.OrdinalIgnoreCase));
        }
        }
    }
}
