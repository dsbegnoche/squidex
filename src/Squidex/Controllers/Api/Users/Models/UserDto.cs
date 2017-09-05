﻿// ==========================================================================
//  UserDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Controllers.Api.Users.Models
{
    public sealed class UserDto
    {
        /// <summary>
        /// The id of the user.
        /// </summary>
        [Required]
        public string Id { get; set; }

        /// <summary>
        /// The email of the user. Unique value.
        /// </summary>
        [Required]
        public string Email { get; set; }

        /// <summary>
        /// The url to the profile picture of the user.
        /// </summary>
        [Required]
        public string PictureUrl { get; set; }

        /// <summary>
        /// The display name (usually first name and last name) of the user.
        /// </summary>
        [Required]
        public string DisplayName { get; set; }

        /// <summary>
        /// The  first name of the user from CPP
        /// </summary>
        [Required]
        public string FirstName { get; set; }

        /// <summary>
        /// The last name of the user from CPP
        /// </summary>
        [Required]
        public string LastName { get; set; }

        /// <summary>
        /// Determines if the user is locked.
        /// </summary>
        [Required]
        public bool IsLocked { get; set; }

        [Required]
        public bool IsAdministrator { get; set; }
    }
}
