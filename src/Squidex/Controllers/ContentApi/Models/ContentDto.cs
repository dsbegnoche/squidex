// ==========================================================================
//  ContentDto.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using NodaTime;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Write.Contents.Commands;
using Squidex.Infrastructure;
using Squidex.Infrastructure.CQRS.Commands;

namespace Squidex.Controllers.ContentApi.Models
{
    public sealed class ContentDto
    {
        /// <summary>
        /// The if of the content element.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The user that has created the content element.
        /// </summary>
        [Required]
        public RefToken CreatedBy { get; set; }

        /// <summary>
        /// The user that has updated the content element.
        /// </summary>
        [Required]
        public RefToken LastModifiedBy { get; set; }

        /// <summary>
        /// The data of the content item.
        /// </summary>
        [Required]
        public object Data { get; set; }

        /// <summary>
        /// The date and time when the content element has been created.
        /// </summary>
        public Instant Created { get; set; }

        /// <summary>
        /// The date and time when the content element has been modified last.
        /// </summary>
        public Instant LastModified { get; set; }

        /// <summary>
        /// The status of the content element.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The version of the content.
        /// </summary>
        public long Version { get; set; }

        public static ContentDto Create(CreateContent command, EntityCreatedResult<NamedContentData> result)
        {
            return ContentDto.Create(command, result, command.Status);
        }

        public static ContentDto Create(ContentCommand command, EntityCreatedResult<NamedContentData> result, Status status = Status.Draft)
        {
            var now = SystemClock.Instance.GetCurrentInstant();

            var response = new ContentDto
            {
                Id = command.ContentId,
                Data = result.IdOrValue,
                Version = result.Version,
                Created = now,
                CreatedBy = command.Actor,
                LastModified = now,
                LastModifiedBy = command.Actor,
                Status = status
            };

            return response;
        }
    }
}
