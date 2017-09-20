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
using Microsoft.Extensions.Options;
using NSwag.Annotations;
using Squidex.Config;
using Squidex.Controllers.Api.UI.Models;
using Squidex.Domain.Apps.Core;
using Squidex.Pipeline;

namespace Squidex.Controllers.Api.UI
{
    /// <summary>
    /// Manages ui settings and configs.
    /// </summary>
    [ApiExceptionFilter]
    [SwaggerTag(nameof(UI))]
    public sealed class UIController : Controller
    {
        private readonly MyUIOptions uiOptions;

        public UIController(IOptions<MyUIOptions> uiOptions)
        {
            this.uiOptions = uiOptions.Value;
        }

        /// <summary>
        /// Get ui settings.
        /// </summary>
        [HttpGet]
        [Route("ui/settings/")]
        [ProducesResponseType(typeof(UISettingsDto), 200)]
        [ApiCosts(0)]
        public IActionResult GetSettings()
        {
            var dto = new UISettingsDto
            {
                RegexSuggestions =
                    uiOptions.RegexSuggestions?
                        .Where(x =>
                            !string.IsNullOrWhiteSpace(x.Name) &&
                            !string.IsNullOrWhiteSpace(x.Pattern))
                        .Select(x => new UIRegexSuggestionDto { Name = x.Name, Pattern = x.Pattern }).ToList()
                    ?? new List<UIRegexSuggestionDto>()
            };

            return Ok(dto);
        }
    }
}