﻿// ==========================================================================
//  MyIdentityOptions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

namespace Squidex.Config.Identity
{
	public sealed class MyIdentityOptions
	{
		public string AdminEmail { get; set; }

		public string AdminPassword { get; set; }

		public string GoogleClient { get; set; }

		public string GoogleSecret { get; set; }

		public string GithubClient { get; set; }

		public string GithubSecret { get; set; }

		public string MicrosoftClient { get; set; }

		public string MicrosoftSecret { get; set; }

        public string AuthorityUrl { get; set; }

		public string CivicPlusIdentityServerBaseUrl { get; set; }

		public string CivicPlusPlatformBaseUrl { get; set; }

		public string CivicPlusAuthority
		{
			get { return $"{CivicPlusIdentityServerBaseUrl}/identity"; }
		}

		public string CivicPlusClient { get; set; }

		public string CivicPlusSecret { get; set; }

		public string CivicPlusPostLogoutRedirectUri { get; set; }

		public string CivicPlusScope { get; set; }

		public string CivicPlusResponseType { get; set; }

		public bool RequiresHttps { get; set; }

		public bool AllowPasswordAuth { get; set; }

		public bool LockAutomatically { get; set; }

		public bool IsAdminConfigured()
		{
			return !string.IsNullOrWhiteSpace(AdminEmail) && !string.IsNullOrWhiteSpace(AdminPassword);
		}

		public bool IsGithubAuthConfigured()
		{
			return !string.IsNullOrWhiteSpace(GithubClient) && !string.IsNullOrWhiteSpace(GithubSecret);
		}

		public bool IsGoogleAuthConfigured()
		{
			return !string.IsNullOrWhiteSpace(GoogleClient) && !string.IsNullOrWhiteSpace(GoogleSecret);
		}

		public bool IsMicrosoftAuthConfigured()
		{
			return !string.IsNullOrWhiteSpace(MicrosoftClient) && !string.IsNullOrWhiteSpace(MicrosoftSecret);
		}

		public bool IsCivicPlusConfigured()
		{
			return 
				!string.IsNullOrWhiteSpace(CivicPlusIdentityServerBaseUrl) &&
				!string.IsNullOrWhiteSpace(CivicPlusPlatformBaseUrl) &&
				!string.IsNullOrWhiteSpace(CivicPlusAuthority) &&
				!string.IsNullOrWhiteSpace(CivicPlusClient) &&
				!string.IsNullOrWhiteSpace(CivicPlusSecret) &&
				!string.IsNullOrWhiteSpace(CivicPlusPostLogoutRedirectUri) &&
				!string.IsNullOrWhiteSpace(CivicPlusScope) &&
				!string.IsNullOrWhiteSpace(CivicPlusResponseType);
		}
	}
}
