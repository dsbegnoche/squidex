﻿// ==========================================================================
//  IUser.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;

namespace Squidex.Shared.Users
{
    public interface IUser
    {
        bool IsLocked { get; }

        string Id { get; }

        string Email { get; }

        string NormalizedEmail { get; }

        IReadOnlyList<Claim> Claims { get; }

        IReadOnlyList<ExternalLogin> Logins { get; }

        IReadOnlyList<string> Roles { get; }

        void UpdateEmail(string email);

        void AddClaim(Claim claim);

        void SetClaim(string type, string value);

        string GetTokenValue(string loginProvider, string name);

        void AddRole(string role);

        void RemoveRole(string role);

        bool InRole(string role);
    }
}
