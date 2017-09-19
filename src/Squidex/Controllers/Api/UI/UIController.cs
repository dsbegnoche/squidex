// ==========================================================================
//  UIController.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================
// CivicPlus - Functionality moved to AppPatternsController.cs

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using NSwag.Annotations;
using Squidex.Controllers.Api.UI.Models;
using Squidex.Infrastructure.CQRS.Commands;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.UI
{
    /// <summary>
    /// Manages ui settings and configs.
    /// </summary>
    [ApiExceptionFilter]
    [AppApi]
    [SwaggerTag(nameof(UI))]
    [MustBeAppReader]
    public sealed class UIController : ControllerBase
    {
        public UIController(ICommandBus commandBus)
            : base(commandBus)
        {
        }

        /// <summary>
        /// Get ui settings.
        /// </summary>
        [HttpGet]
        [Route("ui/{app}/settings/")]
        [ProducesResponseType(typeof(UISettingsDto), 200)]
        [ApiCosts(0)]
        public IActionResult GetSettings()
        {
            var dto = new UISettingsDto
            {
                RegexSuggestions =
                    App.Patterns?
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Name) &&
                            !string.IsNullOrWhiteSpace(x.Pattern))
                        .Select(x => new UIRegexSuggestionDto { Name = x.Name, Pattern = x.Pattern, DefaultMessage = x.DefaultMessage })
                        .OrderBy(x => x.Name)
                        .ToList()
                    ?? new List<UIRegexSuggestionDto>()
            };

            return Ok(dto);
        }
    }
}
