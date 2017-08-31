﻿// ==========================================================================
//  ProfileVM.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Shared.Users;

namespace Squidex.Controllers.UI.Profile
{
    public sealed class ProfileVM
    {
        public string Id { get; set; }

        public string Email { get; set; }

        public string DisplayName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string ErrorMessage { get; set; }

        public string SuccessMessage { get; set; }

        public bool HasPassword { get; set; }

        public bool HasPasswordAuth { get; set; }

        public IReadOnlyList<ExternalLogin> ExternalLogins { get; set; }

        public IReadOnlyList<ExternalProvider> ExternalProviders { get; set; }
    }
}
